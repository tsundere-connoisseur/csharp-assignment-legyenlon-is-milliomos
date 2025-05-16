using LOIM.Game.Controllers;
using Veldrid;
using Veldrid.Sdl2;

namespace LOIM.Game.Display;

public class ImGuiDisplay : IGameDisplay
{
    public bool IsActive => window.Exists;

    private readonly Sdl2Window      window;
    private readonly GraphicsDevice  gd;
    private readonly ImGuiController renderer;
    private readonly CommandList     cl;

    public ImGuiDisplay(Sdl2Window window, GraphicsDevice gd)
    {
        this.window = window;
        this.gd     = gd;

        renderer = new ImGuiController(
                                       gd, gd.MainSwapchain.Framebuffer.OutputDescription,
                                       (int)gd.MainSwapchain.Framebuffer.Width,
                                       (int)gd.MainSwapchain.Framebuffer.Height);

        cl = gd.ResourceFactory.CreateCommandList();
        if (cl is null) throw new NullReferenceException("failed to create command list");

        window.Resized += () => renderer.WindowResized(window.Width, window.Height);
    }


    public void DisplayLine(string line)
    {
        throw new NotImplementedException();
    }

    public void DisplayGrid(ulong rows, ulong columns, params string[] gridItems)
    {
        throw new NotImplementedException();
    }

    public Task<string> Prompt(string promptText)
    {
        throw new NotImplementedException();
    }

    public void DisplayMessage(string message, DisplayMessageType type)
    {
        throw new NotImplementedException();
    }

    public void MainLoopFrameStart()
    {
        var input = window.PumpEvents();
        if (!window.Exists) return;
        renderer.Update(1f / 60f, input); // Compute actual value for deltaSeconds.
    }

    public void MainLoopFrameEnd()
    {
        cl.Begin();
        cl.SetFramebuffer(gd.MainSwapchain.Framebuffer);
        cl.ClearColorTarget(0, RgbaFloat.Black);
        renderer.Render(gd, cl);
        cl.End();
        gd.SubmitCommands(cl);
        gd.SwapBuffers(gd.MainSwapchain);
    }
}
