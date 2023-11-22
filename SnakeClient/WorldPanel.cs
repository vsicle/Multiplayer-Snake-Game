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
using WinRT;

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

        // center the image, draw a wall
        // no logic or math is done here
        canvas.DrawImage(wallImage, -wallImage.Width / 2, -wallImage.Height / 2, wallImage.Width, wallImage.Height);
    }




    private void SnakeSegmentDrawer(object obj, int _id, ICanvas canvas)
    {
        // temproraily draw a circle for the head
        List<Vector2D> vectorList = (List<Vector2D>)obj;

        if (_id % 8 == 0)
        {
            canvas.FillColor = Colors.Blue;
        }
        else if (_id % 8 == 1)
        {
            canvas.FillColor = Colors.Orange;
        }
        else if (_id % 8 == 2)
        {
            canvas.FillColor = Colors.Green;
        }
        else if (_id % 8 == 3)
        {
            canvas.FillColor = Colors.Red;
        }
        else if (_id % 8 == 4)
        {
            canvas.FillColor = Colors.Yellow;
        }
        else if (_id % 8 == 5)
        {
            canvas.FillColor = Colors.Purple;
        }
        else if (_id % 8 == 6)
        {
            canvas.FillColor = Colors.Brown;
        }
        else
        {
            canvas.FillColor = Colors.Aqua;
        }


        if (vectorList[0].X != vectorList[1].X)
        {
            double yCoord = 0;
            double smallerXCoord = 0;
            if (vectorList[0].X < vectorList[1].X)
            {
                smallerXCoord = vectorList[0].X;
                yCoord = vectorList[0].Y;
            }
            else
            {
                smallerXCoord = vectorList[1].X;
                yCoord = vectorList[1].Y;
            }
            // this drawing could be a little off to the side, maybe need to center it?
            canvas.FillRoundedRectangle((float)smallerXCoord, (float)yCoord - 5, (float)Math.Abs(vectorList[0].X - vectorList[1].X), 10, 5);

        }
        else
        {
            double xCoord = 0;
            double smallerYCoord = 0;
            if (vectorList[0].Y < vectorList[1].Y)
            {
                smallerYCoord = vectorList[0].Y;
                xCoord = vectorList[0].X;
            }
            else
            {
                smallerYCoord = vectorList[1].Y;
                xCoord = vectorList[1].X;
            }
            // this drawing could be a little off to the side, maybe need to center it?
            canvas.FillRoundedRectangle((float)xCoord - 5, (float)smallerYCoord, 10, (float)Math.Abs(vectorList[0].Y - vectorList[1].Y), 5);

        }
    }

    private void PowerupDrawer(object obj, ICanvas canvas)
    {
        Powerup p = (Powerup)obj;
        canvas.FillColor = Colors.Gold;
        canvas.FillCircle(0, 0, 8);
        canvas.FillColor = Colors.Red;
        canvas.FillCircle(0, 0, 5);
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
                playerX = (float)tempSnake.body[tempSnake.body.Count - 1].GetX();
                playerY = (float)tempSnake.body[tempSnake.body.Count - 1].GetY();



                canvas.Translate(-playerX + (viewSize / 2), -playerY + (viewSize / 2));

                canvas.DrawImage(background, -theWorld.size / 2, -theWorld.size / 2, theWorld.size, theWorld.size);
                //DrawObjectWithTransform(canvas, tempSnake, 0, 0, 0, SnakeSegmentDrawer);



                //draw walls
                // TODO: nested for loop to draw walls
                // outer loop iterates through walls
                // inner loop runs through the number of actual segments that need to be drawn for 
                // that one wall and calls WallDrawer as many times as needed at the correct coordinates
                lock (theWorld.walls)
                {
                    foreach (Wall wall in theWorld.walls.Values)
                    {

                        //Wall wall = w.Value;

                        double p1x = wall.p1.GetX();
                        double p2x = wall.p2.GetX();

                        double p1y = wall.p1.GetY();
                        double p2y = wall.p2.GetY();

                        double numWalls = 0;

                        // drawing along x-axis
                        if (p1x != p2x)
                        {
                            numWalls = ((int)Math.Abs((p2x - p1x) / 50.0) + 1);

                            for (int i = 0; i < numWalls; i++)
                            {

                                if (p2x - p1x < 0)
                                {
                                    DrawObjectWithTransform(canvas, wall, p1x + (50 * (-i)), p1y, 0, WallDrawer);
                                }
                                else
                                {
                                    DrawObjectWithTransform(canvas, wall, p1x + (50 * i), p1y, 0, WallDrawer);
                                }

                            }
                        }
                        // drawing along y-axis
                        else if (p1y != p2y)
                        {
                            numWalls = ((int)Math.Abs((p2y - p1y) / 50.0) + 1);
                            for (int i = 0; i < numWalls; i++)
                            {
                                if (p2y - p1y < 0)
                                {
                                    DrawObjectWithTransform(canvas, wall, p1x, p1y + (50 * (-i)), 0, WallDrawer);
                                }
                                else
                                {
                                    DrawObjectWithTransform(canvas, wall, p1x, p1y + (50 * i), 0, WallDrawer);
                                }
                            }
                        }
                        else
                        {
                            DrawObjectWithTransform(canvas, wall, p1x, p1y, 0, WallDrawer);
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

                lock (theWorld.snakes)
                {
                    // for each snake
                    foreach (Snake snake in theWorld.snakes.Values)
                    {

                        if (snake.died != true)
                        {

                            int id = snake.snake;
                            // draw each segment
                            for (int i = 1; i < snake.body.Count; i++)
                            {
                                SnakeSegmentDrawer(snake.body.GetRange(i - 1, 2), id, canvas);
                                if (i == snake.body.Count - 1)
                                {
                                    HorizontalAlignment alignment = HorizontalAlignment.Center;
                                    string nameString = snake.name + ": " + snake.score;
                                    canvas.FontColor = Colors.White;
                                    canvas.DrawString(nameString, (float)snake.body[i].GetX(), (float)snake.body[i].GetY()+20, alignment);
                                }
                            }
                        }
                    }
                }

            }
        }
    }

}
