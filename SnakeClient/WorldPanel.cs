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
        // center the image drawing, draw a wall
        // no logic or math is done here
        canvas.DrawImage(wallImage, -wallImage.Width/2, -wallImage.Height/2, wallImage.Width, wallImage.Height);
    }




    private void SnakeSegmentDrawer(object obj, ICanvas canvas)
    {
        // temproraily draw a circle for the head
        Snake s = (Snake)obj;
        canvas.DrawCircle((float)s.body[0].GetX(), (float)s.body[0].GetY(), 20);

        //int snakeSegmentLength = o as int;
        //canvas.DrawLine(0, 0, 0, -snakeSegmentLength);
    }

    private void PowerupDrawer(object obj, ICanvas canvas)
    {
        Powerup p = (Powerup)obj;
        canvas.FillColor = Colors.Gold;
        canvas.FillCircle(0, 0, 8);
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

            if (tempSnake != null && tempSnake.body != null & tempSnake.body[0] != null)
            {
                playerX = (float)tempSnake.body[0].GetX();
                playerY = (float)tempSnake.body[0].GetY();



                canvas.Translate(-playerX + (viewSize / 2), -playerY + (viewSize / 2));

                canvas.DrawImage(background, -theWorld.size / 2, -theWorld.size / 2, theWorld.size, theWorld.size);
                DrawObjectWithTransform(canvas, tempSnake, 0, 0, 0, SnakeSegmentDrawer);



                //draw walls
                // TODO: nested for loop to draw walls
                // outer loop iterates through walls
                // inner loop runs through the number of actual segments that need to be drawn for 
                // that one wall and calls WallDrawer as many times as needed at the correct coordinates
                lock (theWorld.walls)
                {
                    foreach (var w in theWorld.walls)
                    {
                        
                        Wall wall = w.Value;

                        double p1x = wall.p1.GetX();
                        double p2x = wall.p2.GetX();

                        double p1y = wall.p1.GetY();
                        double p2y = wall.p2.GetY();

                        double numWalls = 0;

                        // drawing along x-axis
                        if (p1x != p2x)
                        {
                            numWalls =((int)(p2x - p1x)/50.0);
                            double adjustmentFactor = (p2x - p1x)/25.0;
                            for (int i = 0; i < numWalls; i++)
                            {
                                DrawObjectWithTransform(canvas, wall, p1x+(50*i), p1y, 0, WallDrawer);
                            }
                        }
                        else
                        {
                            numWalls = ((int)(p2y - p1y) / 50.0);
                            double adjustmentFactor = (p2y - p1y) / 50.0;
                            for (int i = 0; i < numWalls; i++)
                            {
                                DrawObjectWithTransform(canvas, wall, p1x, p1y+(50*i), 0, WallDrawer);
                            }
                        }
                    }
                }

                lock (theWorld.powerups)
                {
                    foreach (var p in theWorld.powerups)
                    {
                        Powerup powerup = p.Value;
                        if (powerup.died != true)
                            DrawObjectWithTransform(canvas, powerup, powerup.loc.GetX(), powerup.loc.GetY(), 0, PowerupDrawer);
                    }
                }

                //lock (theWorld.snakes)
                //{
                //    foreach (Snake s in theWorld.snakes.Values)
                //    {
                //        // Loop through snake segments, calculate segment length and segment direction
                //        // Set the Stroke Color, etc, based on s's ID
                //        DrawObjectWithTransform(canvas, segmentLength, segmentX, segmentY, segmentDirection, SnakeSegmentDrawer);
                //    }
                //}
            }
        }
    }

}
