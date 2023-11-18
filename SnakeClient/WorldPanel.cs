using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using IImage = Microsoft.Maui.Graphics.IImage;
#if MACCATALYST
using Microsoft.Maui.Graphics.Platform;
#else
using Microsoft.Maui.Graphics.Win2D;
#endif
using Color = Microsoft.Maui.Graphics.Color;
using System.Reflection;
using Microsoft.Maui;
using System.Net;
using Font = Microsoft.Maui.Graphics.Font;
using SizeF = Microsoft.Maui.Graphics.SizeF;
using Model;
using Windows.ApplicationModel.VoiceCommands;

namespace SnakeGame;
public class WorldPanel : ScrollView, IDrawable
{
    private IImage wallImage;
    private IImage background;

    private World theWorld;

    public delegate void ObjectDrawer(object obj, ICanvas canvas);

    float playerX;
    float playerY;

    private int playerID;

    int viewSize = 900;

    private bool initializedForDrawing = false;

    private IImage loadImage(string name)
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "SnakeClient.Resources.Images";
        using (Stream stream = assembly.GetManifestResourceStream($"{path}.{name}"))
        {
#if MACCATALYST
            return PlatformImage.FromStream(stream);
#else
            return new W2DImageLoadingService().FromStream(stream);
#endif
        }
    }

    public WorldPanel()
    {

    }

    public void SetWorld(World w, int playerID)
    {
        theWorld = w;
        this.playerID = playerID;
        //Snake tempSnake;
        //w.snakes.TryGetValue(playerID, out tempSnake);
        //playerX = (float)tempSnake.body[0].GetX();
        //playerY = (float)tempSnake.body[0].GetY();
    }

    private void InitializeDrawing()
    {
        wallImage = loadImage("wallsprite.png");
        background = loadImage("background.png");
        initializedForDrawing = true;
    }



    /// <summary>
    /// This method performs a translation and rotation to draw an object.
    /// </summary>
    /// <param name="canvas">The canvas object for drawing onto</param>
    /// <param name="o">The object to draw</param>
    /// <param name="worldX">The X component of the object's position in world space</param>
    /// <param name="worldY">The Y component of the object's position in world space</param>
    /// <param name="angle">The orientation of the object, measured in degrees clockwise from "up"</param>
    /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
    private void DrawObjectWithTransform(ICanvas canvas, object o, double worldX, double worldY, double angle, ObjectDrawer drawer)
    {
        // "push" the current transform
        canvas.SaveState();

        canvas.Translate((float)worldX, (float)worldY);
        canvas.Rotate((float)angle);
        drawer(o, canvas);

        // "pop" the transform
        canvas.RestoreState();
    }

    private void WallDrawer(object obj, ICanvas canvas)
    {
        Wall w = (Wall)obj;

        // save x-coordinates of each point
        double p1x = w.p1.GetX();
        double p2x = w.p2.GetX();

        // save y-coordinates of each point
        double p1y = w.p1.GetY();
        double p2y = w.p2.GetY();

        double numWalls = 0;

        if (p1x != p2x)
        {
            numWalls = Math.Abs((p2x - p1x)/50);
            for(int i = 0; i < numWalls; i++)
            {
                double adjustingVariable = ((p2x- p1x)/50)*i;
                //double drawingEndpoint = p2x - p1x;
                //double drawingStartPoint = p1x;
                canvas.DrawImage(wallImage, (float)(p1x+(adjustingVariable*50)), (float)p1y, wallImage.Height, wallImage.Width);
            }
        }
        else
        {
            numWalls = Math.Abs((p2y - p1y));
            for (int i = 0; i < numWalls; i += 25)
            {
                double adjustingVariable = ((p2y - p1y) / 50) * i;
                //double drawingEndpoint = p2x - p1x;
                //double drawingStartPoint = p1x;
                canvas.DrawImage(wallImage, (float)p1x, (float)(p1y+(adjustingVariable*50)), wallImage.Height, wallImage.Width);
            }
        }
    }

    private void SnakeDrawer(object obj, ICanvas canvas)
    {
        Snake s = (Snake)obj;
        canvas.DrawCircle((float)s.body[0].GetX(), (float)s.body[0].GetY(), 20);
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (!initializedForDrawing)
            InitializeDrawing();

        // undo previous transformations from last frame
        canvas.ResetState();

        

        
        if (theWorld != null && theWorld.snakes != null)
        {
            Snake tempSnake;
            theWorld.snakes.TryGetValue(playerID, out tempSnake);

            if (tempSnake != null && tempSnake.body != null &  tempSnake.body[0] != null)
            {
                playerX = (float)tempSnake.body[0].GetX();
                playerY = (float)tempSnake.body[0].GetY();

                

                canvas.Translate(-playerX + (viewSize / 2), -playerY + (viewSize / 2));

                canvas.DrawImage(background, -theWorld.size/2, -theWorld.size/2, theWorld.size, theWorld.size);
                //DrawObjectWithTransform(canvas, wallImage, 100, wallImage.Value.p1.GetY(), 0, WallDrawer);
                // draw snake head
                DrawObjectWithTransform(canvas, tempSnake, 0, 0, 0, SnakeDrawer);



                //draw walls
                lock (theWorld.walls)
                {
                    foreach (var w in theWorld.walls)
                    {
                        Wall wall = w.Value;
                        DrawObjectWithTransform(canvas, wall, wall.p1.GetX(), wall.p1.GetY(), 0, WallDrawer);
                    }
                }
            }
        }


        // example code for how to draw
        // (the image is not visible in the starter code)
        //canvas.DrawImage(wallImage, 0, 0, wallImage.Width, wallImage.Height);
        /*
        // draw the objects in the world
        foreach (var p in theWorld.walls.Values)
            DrawObjectWithTransform(canvas, p,
              p.p1.GetX(), p.p2.GetY(), p.Direction.ToAngle(),
              PlayerDrawer);
        */
    }

}
