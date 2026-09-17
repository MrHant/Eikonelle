using System.Drawing;
using Eikonelle;

namespace Eikonelle.Exams;

// ceps exam for case `editor` (ceps/cases/editor.md).
public class EditorExam
{
    [Fact]
    public void The_editor_draws_and_highlights_over_a_private_copy_of_the_screenshot()
    {
        using Screenshot original = WhiteScreenshot();
        using var editor = new EditorSession(original);

        Draw(editor, EditorTool.Pen, new(5, 10), new(25, 10));
        Draw(editor, EditorTool.Highlighter, new(5, 30), new(25, 30));

        Assert.Equal(Color.White.ToArgb(), original.Image.GetPixel(15, 10).ToArgb());
        Assert.NotEqual(Color.White.ToArgb(), editor.Image.GetPixel(15, 10).ToArgb());
        Assert.NotEqual(Color.White.ToArgb(), editor.Image.GetPixel(15, 30).ToArgb());
    }

    [Fact]
    public void Apply_returns_the_edited_screenshot_for_the_in_memory_preview()
    {
        using Screenshot original = WhiteScreenshot();
        using var editor = new EditorSession(original);
        Draw(editor, EditorTool.Pen, new(5, 10), new(25, 10));

        using Screenshot applied = editor.Apply();
        var preview = new PreviewModel();
        preview.Show(applied);

        Assert.Same(applied, preview.Current);
        Assert.NotEqual(Color.White.ToArgb(), preview.Current!.Image.GetPixel(15, 10).ToArgb());
    }

    [Theory]
    [InlineData(EditorTool.Line)]
    [InlineData(EditorTool.Rectangle)]
    [InlineData(EditorTool.Ellipse)]
    public void The_editor_draws_geometrical_shapes(EditorTool tool)
    {
        using Screenshot original = WhiteScreenshot();
        using var editor = new EditorSession(original);

        Draw(editor, tool, new(10, 10), new(40, 40));

        Assert.Contains(Pixels(editor.Image), color => color.ToArgb() != Color.White.ToArgb());
    }

    [Fact]
    public void Apply_returns_the_edited_in_memory_screenshot_without_changing_the_original()
    {
        using Screenshot original = WhiteScreenshot();
        using var editor = new EditorSession(original);
        Draw(editor, EditorTool.Line, new(5, 5), new(30, 5));

        using Screenshot applied = editor.Apply();

        Assert.Equal(Color.White.ToArgb(), original.Image.GetPixel(15, 5).ToArgb());
        Assert.NotEqual(Color.White.ToArgb(), applied.Image.GetPixel(15, 5).ToArgb());
    }

    [Theory]
    [InlineData(EditorTool.Pen)]
    [InlineData(EditorTool.Highlighter)]
    [InlineData(EditorTool.Line)]
    [InlineData(EditorTool.Rectangle)]
    [InlineData(EditorTool.Ellipse)]
    public void Dragging_previews_each_update_before_mouse_release(EditorTool tool)
    {
        using Screenshot original = WhiteScreenshot();
        using var editor = new EditorSession(original);
        var previews = new List<int[]>();
        editor.Changed += (_, _) => previews.Add(Pixels(editor.Image).Select(color => color.ToArgb()).ToArray());

        editor.BeginGesture(tool, new(10, 10));
        editor.UpdateGesture(new(25, 25));
        editor.UpdateGesture(new(40, 40));

        Assert.Equal(2, previews.Count);
        Assert.All(previews, pixels => Assert.Contains(pixels, pixel => pixel != Color.White.ToArgb()));
        Assert.False(previews[0].SequenceEqual(previews[1]));
        Assert.All(Pixels(original.Image), color => Assert.Equal(Color.White.ToArgb(), color.ToArgb()));
    }

    [Theory]
    [InlineData(EditorTool.Pen)]
    [InlineData(EditorTool.Highlighter)]
    public void Freehand_tools_follow_the_drag_path(EditorTool tool)
    {
        using Screenshot original = WhiteScreenshot();
        using var editor = new EditorSession(original);

        editor.BeginGesture(tool, new(10, 10));
        editor.UpdateGesture(new(10, 40));
        editor.EndGesture(new(40, 40));

        Assert.NotEqual(Color.White.ToArgb(), editor.Image.GetPixel(10, 25).ToArgb());
        Assert.NotEqual(Color.White.ToArgb(), editor.Image.GetPixel(25, 40).ToArgb());
        Assert.Equal(Color.White.ToArgb(), editor.Image.GetPixel(25, 25).ToArgb());
    }

    [Theory]
    [InlineData(EditorTool.Pen)]
    [InlineData(EditorTool.Line)]
    [InlineData(EditorTool.Rectangle)]
    [InlineData(EditorTool.Ellipse)]
    public void Pen_and_shapes_have_opaque_red_four_pixel_strokes(EditorTool tool)
    {
        using Screenshot original = WhiteScreenshot();
        using var editor = new EditorSession(original);
        Draw(editor, tool, new(10, 10), new(40, tool is EditorTool.Pen or EditorTool.Line ? 10 : 40));

        Assert.Equal(Color.Red.ToArgb(), editor.Image.GetPixel(25, 10).ToArgb());
        // Sum coverage across the top stroke, including antialiased edge pixels.
        double width = Enumerable.Range(5, 11)
            .Sum(y => (255 - editor.Image.GetPixel(25, y).G) / 255.0);
        Assert.InRange(width, 3.8, 4.2);
        Assert.Equal(Color.White.ToArgb(), editor.Image.GetPixel(25, 25).ToArgb());
    }

    [Fact]
    public void Highlighter_has_a_translucent_yellow_eighteen_pixel_stroke()
    {
        using Screenshot original = WhiteScreenshot();
        using var editor = new EditorSession(original);
        Draw(editor, EditorTool.Highlighter, new(10, 25), new(40, 25));

        Color center = editor.Image.GetPixel(25, 25);
        Assert.Equal(255, (int)center.R);
        Assert.True(center.R > center.G && center.G > center.B);
        Assert.InRange((int)center.B, 1, 254); // White remains visible through the yellow.
        double width = Enumerable.Range(0, 50)
            .Sum(y => (255 - editor.Image.GetPixel(25, y).B) / (double)(255 - center.B));
        Assert.InRange(width, 17.8, 18.2);
    }

    [Theory]
    [InlineData(EditorTool.Line, 25, 25)]
    [InlineData(EditorTool.Rectangle, 10, 25)]
    [InlineData(EditorTool.Ellipse, 10, 25)]
    public void Shapes_follow_their_geometry_and_replace_the_previous_drag_preview(EditorTool tool, int x, int y)
    {
        using Screenshot original = WhiteScreenshot();
        using var editor = new EditorSession(original);
        editor.BeginGesture(tool, new(10, 10));
        editor.UpdateGesture(new(40, 40));
        Assert.Equal(Color.Red.ToArgb(), editor.Image.GetPixel(x, y).ToArgb());

        editor.UpdateGesture(new(20, 20));
        Assert.Equal(Color.White.ToArgb(), editor.Image.GetPixel(x, y).ToArgb());
    }

    [Fact]
    public void The_preview_menu_has_an_item_that_opens_the_editor()
    {
        var opened = false;
        AppMenu menu = AppMenu.ForPreview(openEditor: () => opened = true, openSettings: () => { });

        Assert.Single(menu.Items, item => item.Text == "Editor").Invoke();

        Assert.True(opened);
    }

    private static void Draw(EditorSession editor, EditorTool tool, EditorPoint start, EditorPoint end)
    {
        editor.BeginGesture(tool, start);
        editor.EndGesture(end);
    }

    private static Screenshot WhiteScreenshot()
    {
        var bitmap = new Bitmap(50, 50);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.White);
        return new Screenshot(bitmap);
    }

    private static IEnumerable<Color> Pixels(Bitmap bitmap)
    {
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                yield return bitmap.GetPixel(x, y);
            }
        }
    }
}
