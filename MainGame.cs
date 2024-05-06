﻿#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpeedRacer;

public class NoLevelsException : Exception
{
    
}
public class MainGame : Game
{
    protected int[] CharPos;
    protected GraphicsDeviceManager Graphics;
    protected SpriteBatch? SpriteBatch;
    protected BasicEffect? BasicEffect;
    protected int[][] CurrentMap;
    protected int[][][] AllMaps;
    protected Dictionary<string, int> Settings = new ();
    
    public MainGame()
    {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        CharPos = new [] {50, 50};
        CurrentMap = new int[][] {};
    }

    public static string SelectMenu(string[]? options = null, string prompt = "Please select an option:", ConsoleColor highlightOption = ConsoleColor.Blue) //Use arrow keys to select an option from a menu. Console only.
    {
        options ??= new [] { "Yes", "No" };

        int pos = 0;
        ConsoleColor defaultBackgroundColor = Console.BackgroundColor;
        ConsoleColor defaultForegroundColor = Console.ForegroundColor;
        bool selectedOption = false;
        while (!selectedOption)
        {
            Console.BackgroundColor = defaultBackgroundColor;
            Console.ForegroundColor = defaultForegroundColor;
            Console.Clear();
            Console.WriteLine(prompt);
            for (int i = 0; i < options.Length; i++)
            {
                if (i == pos)
                {
                    Console.BackgroundColor = highlightOption;
                    Console.WriteLine($" > {options[i]}");
                }
                else
                {
                    Console.BackgroundColor = defaultBackgroundColor;
                    Console.WriteLine($" • {options[i]}");
                }
            }

            Console.BackgroundColor = defaultBackgroundColor;
            Console.WriteLine("Use arrow keys to move. Press space to select.");
            ConsoleKey currentKey = Console.ReadKey().Key;
            switch (currentKey)
            {
                case ConsoleKey.DownArrow:
                    pos++;
                    if (pos >= options.Length)
                    {
                        pos = 0;
                    }

                    break;
                case ConsoleKey.UpArrow:
                    pos--;
                    if (pos < 0)
                    {
                        pos = options.Length - 1;
                    }

                    break;
                case ConsoleKey.Spacebar:
                    selectedOption = true;
                    break;
            }
        }

        return options[pos];
        }
    protected void DrawRectangle(float x, float y, float width, float height, Color color) //x and y are positions for the upper-left hand corner.
    {
        VertexPositionColor[] verticesA = new VertexPositionColor[3];
        VertexPositionColor[] verticesB = new VertexPositionColor[3];
        verticesA[0] = new VertexPositionColor(new Vector3(x,y, 0), color);
        verticesA[1] = new VertexPositionColor(new Vector3(x + width, y, 0), color);
        verticesA[2] = new VertexPositionColor(new Vector3(x, y + height, 0), color);
        verticesB[0] = new VertexPositionColor(new Vector3(x + width,y +height, 0), color);
        verticesB[1] = new VertexPositionColor(new Vector3(x, y + height, 0), color);
        verticesB[2] = new VertexPositionColor(new Vector3(x+width, y, 0), color);
        GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verticesA, 0, 1);
        GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verticesB, 0, 1);
    }

    protected override void Initialize()
    {
        try
        {
            string[] games = Directory.GetDirectories("../../../levels");
            if (games.Length == 0)
            {
                throw new NoLevelsException();
            }

            List<string> errors = new List<string> ();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.BackgroundColor = ConsoleColor.Black;
            string toPlay = SelectMenu(games, "What campaign would you like to play?", ConsoleColor.Yellow);
            try
            {
                string[] rawSettings = File.ReadAllLines($"../../../levels/{toPlay}/settings.txt");
                foreach (string rawSetting in rawSettings)
                {
                    try
                    {
                        string[] temp = rawSetting.Split(": ");
                        Settings.Add(temp[0], int.Parse(temp[1]));
                    }
                    catch (FormatException e)
                    {
                        errors.Add($"Your value for setting ({rawSetting}) is not numerical (cannot be turned into integer): {e}");
                    }
                    catch (IndexOutOfRangeException)
                    {
                        errors.Add($"Your setting ({rawSetting}) does not contain a \": \". Please make sure to add that between the name of the setting and the value");
                    }
                    catch (Exception e)
                    {
                        errors.Add($"Setting ({rawSetting}) could not be read: {e}");
                    }
                }
            }
            catch (FileNotFoundException)
            {
                errors.Add($"Settings file not found: please make sure levels/{toPlay}/settings.txt exists.");
            }
        }
        catch (NoLevelsException)
        {
            Console.WriteLine("Sorry, there are no campaigns available to play. Please add a campaign inside /levels. (view https://github.com/a-727/SpeedRacer/blob/main/README.md for more info)");
            Console.Write("Press enter to continue:");
            Exit();
            Console.ReadLine();
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine("The levels directory does not exist. Creating it... You should probably add some campaigns inside /levels - please view https://github.com/a-727/SpeedRacer/blob/main/README.md");
            Directory.CreateDirectory("../../../levels");
        }
        base.Initialize();
    }
    
    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        BasicEffect = new BasicEffect(GraphicsDevice);
        BasicEffect.VertexColorEnabled = true;
        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        BasicEffect!.Projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1);
        GraphicsDevice.Clear(Color.CornflowerBlue);
        foreach (EffectPass pass in BasicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            DrawRectangle(50, 50, 100, 200, Color.Blue);
            DrawRectangle(150, 250, 400, 35, Color.Yellow);
        }
        // TODO: Add your drawing code here
        SpriteBatch!.Begin();
        SpriteBatch.End();
        base.Draw(gameTime);
    }
}
