#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpeedRacer;

public class NoLevelsException : Exception
{
    
}

public class Clock
{
    private int _secs = 0;
    private int _tenth = 0;
    private int _hundredth = 0;
    private double _mili = 0;
    public Clock(double startingMilliseconds = 0)
    {
        IncrementMilliseconds(startingMilliseconds);
    }
    public void IncrementSeconds(int by)
    {
        _secs += by;
    }
    public void IncrementTenths(int by)
    {
        _tenth += by;
        while (_tenth >= 10)
        {
            _tenth -= 10;
            IncrementSeconds(1);
        }
    }
    public void IncrementHundredths(int by)
    {
        _hundredth += by;
        while (_hundredth >= 10)
        {
            _hundredth -= 10;
            IncrementTenths(1);
        }
    }
    public void IncrementMilliseconds(double by)
    {
        _mili += by;
        while (_mili >= 10)
        {
            _mili -= 10;
            IncrementHundredths(1);
        }
    }
    public string View()
    {
        return $"{_secs}.{_tenth}{_hundredth}{_mili}";
    }
}
public class MainGame : Game
{
    protected float[] CharPos;
    protected int DiffMultiplier;
    protected GraphicsDeviceManager Graphics;
    protected SpriteBatch? SpriteBatch;
    protected BasicEffect? BasicEffect;
    protected int[][] CurrentMap;
    protected float xMovement; //x movement multiplier (-1 for left, 0 for stopped, or 1 for right) - set to half value when bounced.
    protected float yMovement; //y movement multiplier (-1 for up, 0 for stopped, or 1 for down) - set to half value when bounced.
    protected int[][][] AllMaps;
    protected Dictionary<string, int> Settings;
    protected List<string> blurbs;
    protected int level;
    protected Clock TimeClock;
    protected string currentBlurb;
    protected Random Random;
    protected bool LastFrameWall = false;
    private string _versionName = "Speed Racer v0.2.0.0 (Build 31)";
    protected Color[] IntToColor = new[] {Color.White, Color.Black, new Color(0, 255, 0), new Color(255, 0, 255), new Color(255, 0, 0)};
    
    public MainGame()
    {
        Random = new Random();
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        CharPos = [50, 50];
        Settings = new Dictionary<string, int>();
        blurbs = DefaultBlurbs();
        level = 1;
        TimeClock = new Clock();
    }

    protected Dictionary<string, int[]> SetDefaultValues()
    {
        Dictionary<string, int[]> toReturn = new ();
        //At the start settings
        toReturn.Add("!Levels", [1, 100, 5, 1]);
        toReturn.Add("!xSize", [4, 500, 20, 1]);
        toReturn.Add("!ySize", [4, 400, 20, 1]);
        toReturn.Add("PlayerSize", [25, 200, 55, 0]);
        toReturn.Add("ShowTimer", [0, 1, 1, 0]);
        toReturn.Add("SaveToLeaderboard", [0, 1, 0, 0]);
        toReturn.Add("AllowLeaderboardClear", [0, 1, 0, 0]);
        toReturn.Add("DiffSpeedMultiplier", [30, 250, 80, 0]);
        toReturn.Add("AllowSuperEasyMode", [0, 1, 0, 0]);
        toReturn.Add("AllowSuperHardMode", [0, 1, 0, 0]);
        toReturn.Add("finishedProject", [0, 1, 0, 0]);
        toReturn.Add("allowEasyMode", [0, 1, 1, 0]);
        toReturn.Add("allowNormalMode", [0, 1, 1, 0]);
        toReturn.Add("allowHardMode", [0, 1, 1, 0]);
        //v0.1.0 settings
        toReturn.Add("AllowDiagonal", [0, 1, 0, 0]);
        toReturn.Add("maxMillisecondsPerFrame", [10, 1000, 100, 0]);
        //v0.1.1 settings
        toReturn.Add("AllowPause", [0, 1, 1, 0]);
        toReturn.Add("pasueClockWithoutMovement", [0, 1, 1, 0]);
        return toReturn;
    }

    protected List<string> DefaultBlurbs()
    {
        return [
        "v0.1.0 is the first release of the game that draws the console",
        "Use arrow keys to steer",
        "Originally in python",
        "Don't hit the red (duh)",
        "Bounce on the walls (black)"
        ];
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
        Window.Title = $"{_versionName} - Loading";
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
        }
        catch (NoLevelsException)
        {
            Console.WriteLine("Sorry, there are no campaigns available to play. Please add a campaign inside /levels. (view https://github.com/a-727/SpeedRacer/blob/main/README.md for more info)");
            Console.Write("Press enter to continue:");
            Exit();
            Console.ReadLine();
            return;
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine("The levels directory does not exist. Creating it... You should probably add some campaigns inside /levels - please view https://github.com/a-727/SpeedRacer/blob/main/README.md");
            Directory.CreateDirectory("../../../levels");
            Initialize();
        }
        foreach (KeyValuePair<string, int[]> current in SetDefaultValues())
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
        bool keepGoing = true;
        AllMaps = new int[Settings["!Levels"]][][];
        for (int i = 1; (keepGoing && i <= Settings["!Levels"]); i++)
        {
            try
            {
                AllMaps[i-1] = new int[Settings["!xSize"]][];
                string[] lines = File.ReadAllLines($"../../../levels/{toPlay}/{i}.csv");
                for (int j = 0; j < Settings["!xSize"]; j++)
                {
                    AllMaps[i-1][j] = new int[Settings["!ySize"]];
                    for (int k = 0; k < Settings["!ySize"]; k++)
                    {
                        try
                        {
                            AllMaps[i-1][j][k] = int.Parse(lines[k].Split(",")[j]);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            AllMaps[i-1][j][k] = 0;
                            errors.Add($"Cannot find position ({j + 1}, {k + 1}) in csv file for level {i}.");
                        }
                        catch (FormatException)
                        {
                            AllMaps[i][j][k] = 0;
                            errors.Add($"In level {i}, at position ({j + 1}, {k + 1}), the value {lines[j].Split(',')[k]} could not be converted to integer. Make sure it is an integer of valid value (0-4 for base game, more for some mods).");
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                errors.Add($"Level {i} (filename: {i}.csv) was not found. Please make sure it exists. The game cannot play if all levels are not found.");
                Exit();
            }
        }
        SetupLevel(1);
        base.Initialize();
        List<string> diffOptions = [];
        if (Settings["AllowSuperEasyMode"] == 1)
        {
            diffOptions.Add("Super Easy");
        }
        if (Settings["allowEasyMode"] == 1)
        {
            diffOptions.Add("Easy");
        }
        if (Settings["allowNormalMode"] == 1)
        {
            diffOptions.Add("Medium");
        }
        if (Settings["allowHardMode"] == 1)
        {
            diffOptions.Add("Hard");
        }
        if (Settings["AllowSuperHardMode"] == 1)
        {
            diffOptions.Add("Super Hard");
        }
        if (diffOptions.Count == 0)
        {
            diffOptions = ["Easy", "Medium", "Hard"];
        }
        string diffChosen = SelectMenu(diffOptions.ToArray(), "Please select a difficulty");
        DiffMultiplier = diffChosen switch { "Super Easy" => 2, "Easy" => 3, "Medium" => 4, "Hard" => 5, "Super Hard" => 7, _ => 4 };
        if (errors.Count == 0) {}
        else if (Settings["finishedProject"] == 0)
        {
            foreach (string i in errors)
            {
                Console.WriteLine(i);
            }
        }
        else
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = 0;
            Console.Write(
                "This campagn generated errors during setup. If this is your campaign, please enter debug to view errors (anything else to continue): ");
            if (Console.ReadLine() == "debug")
            {
                foreach (string i in errors)
                {
                    Console.WriteLine(i);
                }
            }
        }
    }
    
    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        BasicEffect = new BasicEffect(GraphicsDevice);
        BasicEffect.VertexColorEnabled = true;
        // TODO: use this.Content to load your game content here
    }

    protected bool[] GetCollisions(int numberOfBlocks, int[][]? map = null, float[]? playerPosition = null)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Space))
        {
            Console.Write("");
        }
        if (map is null)
        {
            map = CurrentMap;
        }
        if (playerPosition is null)
        {
            playerPosition = CharPos;
        }
        bool[] toReturn = new bool[numberOfBlocks];
        float pSize = Settings["PlayerSize"]/(float)100;
        toReturn[map[(int)Math.Floor(CharPos[0])][(int)Math.Floor(CharPos[1])]] = true;
        toReturn[map[(int)Math.Floor(CharPos[0]+pSize)][(int)Math.Floor(CharPos[1])]] = true;
        toReturn[map[(int)Math.Floor(CharPos[0])][(int)Math.Floor(CharPos[1]+pSize)]] = true;
        toReturn[map[(int)Math.Floor(CharPos[0]+pSize)][(int)Math.Floor(CharPos[1]+pSize)]] = true;
        return toReturn;
    }
    
    protected override void Update(GameTime gameTime)
    {
        KeyboardState state = Keyboard.GetState();
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || state.IsKeyDown(Keys.Escape))
        {
            Exit();
        }
        
        if (state.IsKeyDown(Keys.Space) && Settings["AllowPause"] == 1)
        {
            xMovement = 0;
            yMovement = 0;
        }
        float millisecondsElapsed = gameTime.ElapsedGameTime.Milliseconds;
        if (millisecondsElapsed > Settings["maxMillisecondsPerFrame"])
        {
            millisecondsElapsed = Settings["maxMillisecondsPerFrame"];
        }
        float movementMultiplier = millisecondsElapsed*Settings["DiffSpeedMultiplier"]*DiffMultiplier/100000;
        bool[] check = GetCollisions(5);
        if (check[4])
        {
            SetupLevel(level);
        }
        else if (check[2])
        {
            level++;
            try
            {
                SetupLevel(level);
            }
            catch (IndexOutOfRangeException)
            {
                Exit();
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("You win!");
            }
        }
        else if (check[1] && !LastFrameWall)
        {
            LastFrameWall = true;
            if (xMovement > 0)
            {
                xMovement = (float)-0.5;
            }
            else if (xMovement < 0)
            {
                xMovement = (float)0.5;
            }

            if (yMovement > 0)
            {
                yMovement = (float)-0.5;
            }
            else if (yMovement < 0)
            {
                yMovement = (float)0.5;
            }
        }
        else if (!check[1])
        {
            LastFrameWall = false;
            if (state.IsKeyDown(Keys.Right))
            {
                xMovement = 1;
                if (Settings["AllowDiagonal"] == 0)
                {
                    yMovement = 0;
                }
            }
            if (state.IsKeyDown(Keys.Left))
            {
                xMovement = -1;
                if (Settings["AllowDiagonal"] == 0)
                {
                    yMovement = 0;
                }
            }
            if (state.IsKeyDown(Keys.Up))
            {
                yMovement = -1;
                if (Settings["AllowDiagonal"] == 0)
                {
                    xMovement = 0;
                }
            }
            if (state.IsKeyDown(Keys.Down))
            {
                yMovement = 1;
                if (Settings["AllowDiagonal"] == 0)
                {
                    xMovement = 0;
                }
            }
        }
        CharPos[0] += xMovement * movementMultiplier;
        CharPos[1] += yMovement * movementMultiplier;
        TitleCard();
        TimeClock.IncrementMilliseconds(millisecondsElapsed);
        base.Update(gameTime);
    }
    
    protected float PixelSize(int x_total, int y_total)
    {
        float a = (float)Window.ClientBounds.Width / x_total;
        float b = (float)Window.ClientBounds.Width / y_total;
        if (a < b)
        {
            return a;
        }
        return b;
    }
    
    protected override void Draw(GameTime gameTime)
    {
        BasicEffect!.Projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1);
        GraphicsDevice.Clear(Color.CornflowerBlue);
        foreach (EffectPass pass in BasicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            float size = PixelSize(Settings["!xSize"], Settings["!ySize"]);
            for (int i = 0; i < CurrentMap.Length; i++)
            {
                for (int j = 0; j < CurrentMap[i].Length; j++)
                {
                    DrawRectangle(i*size, j*size, size, size, IntToColor[CurrentMap[i][j]]);
                }
            }
            float actualPSize = size * (Settings["PlayerSize"]/(float)100);
            DrawRectangle(CharPos[0] * size, CharPos[1] * size, actualPSize, actualPSize, Color.Blue);
        }
        SpriteBatch!.Begin();
        SpriteBatch.End();
        base.Draw(gameTime);
    }

    protected void SetupLevel(int level)
    {
        xMovement = 0;
        yMovement = 0;
        CurrentMap = AllMaps[level-1];
        bool toBreak = false;
        for (int i = 0; i < CurrentMap.Length; i++)
        {
            for (int j = 0; j < CurrentMap[i].Length; j++)
            {
                if (CurrentMap[i][j] == 3)
                {
                    CharPos = [i+(float)0.5-(Settings["PlayerSize"]/(float)200), j+(float)0.5-(Settings["PlayerSize"]/(float)200)];
                    toBreak = true;
                    break;
                }
                if (toBreak)
                {
                    break;
                }
            }
        }
        currentBlurb = blurbs[Random.Next(0, blurbs.Count)];
    }

    protected void TitleCard()
    {
        string windowTitle = $"{_versionName} - Level {level} - {currentBlurb}";
        if (Settings["ShowTimer"] == 1)
        {
            windowTitle += $" - {TimeClock.View()}";
        }
        Window.Title = windowTitle;
    }
}
