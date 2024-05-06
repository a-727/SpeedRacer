#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    protected double[] CharPos;
    protected GraphicsDeviceManager Graphics;
    protected SpriteBatch? SpriteBatch;
    protected BasicEffect? BasicEffect;
    protected int[][] CurrentMap;
    protected int[][][] AllMaps;
    protected Dictionary<string, int> Settings;
    protected Dictionary<string, int[]> DefaultSettings;
    
    public MainGame()
    {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        CharPos = [50, 50];
        Settings = new Dictionary<string, int>();
        DefaultSettings = SetDefaultValues();
    }

    protected Dictionary<string, int[]> SetDefaultValues()
    {
        Dictionary<string, int[]> toReturn = new ();
        toReturn.Add("!Levels", [1, 100, 5, 1]);
        toReturn.Add("!xSize", [4, 500, 20, 1]);
        toReturn.Add("!ySize", [4, 400, 20, 1]);
        toReturn.Add("PlayerSize", [45, 200, 65, 0]);
        toReturn.Add("ShowTimer", [0, 1, 1, 0]);
        toReturn.Add("SaveToLeaderboard", [0, 1, 0, 0]);
        toReturn.Add("AllowLeaderboardClear", [0, 1, 0, 0]);
        toReturn.Add("DiffSpeedMultiplier", [10, 100, 25, 0]);
        toReturn.Add("AllowSuperEasyMode", [0, 1, 0, 0]);
        toReturn.Add("AllowSuperHardMode", [0, 1, 0, 0]);
        toReturn.Add("finishedProject", [0, 1, 0, 0]);
        toReturn.Add("allowEasyMode", [0, 1, 1, 0]);
        toReturn.Add("allowNormalMode", [0, 1, 1, 0]);
        toReturn.Add("allowHardMode", [0, 1, 1, 0]);
        return toReturn;
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
        string toPlay = "none";
        List<string> errors = new List<string> ();
        try
        {
            string[] fullPathToGames = Directory.GetDirectories("../../../levels");
            string[] games = new string[fullPathToGames.Length];
            for (int i = 0; i < fullPathToGames.Length; i++)
            {
                games[i] = Path.GetRelativePath("../../../levels", fullPathToGames[i]);
            }
            if (games.Length == 0)
            {
                throw new NoLevelsException();
            }
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.BackgroundColor = ConsoleColor.Black;
            toPlay = SelectMenu(games, "What campaign would you like to play?", ConsoleColor.Yellow);
            try
            {
                string[] rawSettings = File.ReadAllLines($"../../../levels/{toPlay}/settings.txt");
                foreach (string rawSetting in rawSettings)
                {
                    try
                    {
                        string[] temp = rawSetting.Split(" = ");
                        Settings.Add(temp[0], int.Parse(temp[1]));
                    }
                    catch (FormatException e)
                    {
                        errors.Add($"Your value for setting ({rawSetting}) is not numerical (cannot be turned into integer): {e}");
                    }
                    catch (IndexOutOfRangeException)
                    {
                        errors.Add($"Your setting ({rawSetting}) does not contain a \" = \". Please make sure to add that between the name of the setting and the value");
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
            foreach (KeyValuePair<string, int[]> current in DefaultSettings)
            {
                if (Settings.ContainsKey(current.Key))
                {
                    if (Settings[current.Key] < current.Value[0])
                    {
                        errors.Add($"Setting {current.Key} was set to value {Settings[current.Key]}, which was below the minimum value {current.Value[0]}.");
                        Settings[current.Key] = current.Value[2];
                    }
                    else if (Settings[current.Key] > current.Value[1])
                    {
                        errors.Add($"Setting {current.Key} was set to value {Settings[current.Key]}, which was above the maximum value {current.Value[1]}.");
                        Settings[current.Key] = current.Value[2];
                    }
                }
                else
                {
                    Settings.Add(current.Key, current.Value[2]);
                    if (current.Value[3] == 1)
                    {
                        errors.Add($"Setting {current.Key} had no value. This setting should be set for every campaign.");
                    }
                }
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
            Initialize();
        }
        foreach (string i in errors)
        {
            Console.WriteLine(i);
        }
        bool keepGoing = true;
        for (int i = 1; (keepGoing && i <= Settings["Levels"]); i++)
        {
            try
            {
                AllMaps[i] = new int[Settings["xSize"]][];
                string[] lines = File.ReadAllLines($"../../../levels/{toPlay}/{i}.csv");
                for (int j = 0; j < Settings["xSize"]; j++)
                {
                    AllMaps[i][j] = new int[Settings["ySize"]];
                    for (int k = 0; k < Settings["ySize"]; k++)
                    {
                        
                    }
                }
            }
            catch (FileNotFoundException)
            {
                
            }
        }
        SetupLevel(1);
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

    protected void SetupLevel(int level)
    {
        CurrentMap = AllMaps[level];
        bool toBreak = false;
        for (int i = 0; i < CurrentMap.Length; i++)
        {
            for (int j = 0; j < CurrentMap[i].Length; j++)
            {
                if (CurrentMap[i][j] == 3)
                {
                    CharPos = [0.5+i-(Settings["xSize"]/200), 0.5+j-(Settings["ySize"]/200)];
                    toBreak = true;
                    break;
                }
                if (toBreak)
                {
                    break;
                }
            }
        }
    }
}
