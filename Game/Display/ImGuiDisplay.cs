using System.Numerics;
using ImGuiNET;
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
        ImGui.Text(line);
    }

    public void DisplayGrid(ulong rows, ulong columns, bool appendLetters = true, params string[] gridItems)
    {
        if (!ImGui.BeginTable($"##grid{gridItems.GetHashCode()}", (int)columns, ImGuiTableFlags.SizingFixedFit)) return;

        char letter = 'A';
        
        for (ulong row = 0; row < rows; row++)
        {
            ImGui.TableNextRow();
            for (ulong column = 0; column < columns; column++)
            {
                ImGui.TableNextColumn();
                var idx = column * rows + row;
                if (idx >= (ulong)gridItems.LongLength) break;
                if (appendLetters)
                {
                    ImGui.Text($"{letter}: ");
                    ImGui.SameLine();
                }
                ImGui.Text(gridItems[idx]);
                letter++;
            }
        }

        ImGui.EndTable();
    }

    public bool Prompt(string promptText, ref string output)
    {
        return ImGui.InputText(promptText, ref output, 255,
                               ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CharsUppercase |
                               ImGuiInputTextFlags.CharsNoBlank);
    }

    public void DisplayMessage(string message, DisplayMessageType type)
    {
        // throw new NotImplementedException();
    }

    public void MainLoopFrameStart()
    {
        var input = window.PumpEvents();
        if (!window.Exists) return;
        renderer.Update(1f / 60f, input); // Compute actual value for deltaSeconds.
        var open = true;
        ImGui.SetNextWindowSize(ImGui.GetWindowViewport().Size);
        ImGui.SetNextWindowPos(Vector2.Zero);
        ImGui.Begin("game", ref open, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);
    }

    public void MainLoopFrameEnd()
    {
        ImGui.End();
        cl.Begin();
        cl.SetFramebuffer(gd.MainSwapchain.Framebuffer);
        cl.ClearColorTarget(0, RgbaFloat.Black);
        renderer.Render(gd, cl);
        cl.End();
        gd.SubmitCommands(cl);
        gd.SwapBuffers(gd.MainSwapchain);
    }
}
