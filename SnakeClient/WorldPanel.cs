﻿using System.Collections.Generic;
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

namespace SnakeGame;
public class WorldPanel : ScrollView, IDrawable
{
    private IImage wall;
    private IImage background;

    private World theWorld;

    float playerX;
    float playerY;

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
        Snake tempSnake;
        w.snakes.TryGetValue(playerID, out tempSnake);
        playerX = (float)tempSnake.body[0].GetX();
        playerY = (float)tempSnake.body[0].GetY();
    }

    private void InitializeDrawing()
    {
        wall = loadImage( "wallsprite.png" );
        background = loadImage( "background.png" );
        initializedForDrawing = true;
    }

    /*

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

    */

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if ( !initializedForDrawing )
            InitializeDrawing();

        

        // undo previous transformations from last frame
        canvas.ResetState();

        canvas.Translate(-playerX + (viewSize / 2), -playerY + (viewSize / 2));

        // example code for how to draw
        // (the image is not visible in the starter code)
        canvas.DrawImage(wall, 0, 0, wall.Width, wall.Height);
        /*
        // draw the objects in the world
        foreach (var p in theWorld.walls.Values)
            DrawObjectWithTransform(canvas, p,
              p.p1.GetX(), p.p2.GetY(), p.Direction.ToAngle(),
              PlayerDrawer);
        */
    }

}
