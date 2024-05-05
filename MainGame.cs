#nullable enable
using System;
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
    private int[] _charPos;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;
    private BasicEffect? _basicEffect;
    private int[][] _map;

    
    public MainGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _charPos = new [] {50, 50};
        _map = new int[][] {};
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
    private void DrawRectangle(float x, float y, float width, float height, Color color) //x and y are positions for the upper-left hand corner.
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
            string[] games = Directory.GetDirectories("../../../t");
            if (games.Length == 0)
            {
                throw new NoLevelsException();
            }
        }
        catch
        {
            Console.WriteLine("Sorry, there are no campaigns available to play. Please add a level inside /gameLevels. You should find plenty in https://github.com/a-727/SpeedRacer.");
            Console.Write("Press enter to continue:");
            Console.ReadLine();
            
        }

        base.Initialize();
    }
    
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _basicEffect = new BasicEffect(GraphicsDevice);
        _basicEffect.VertexColorEnabled = true;
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
        _basicEffect!.Projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1);
        GraphicsDevice.Clear(Color.CornflowerBlue);
        foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            DrawRectangle(50, 50, 100, 200, Color.Blue);
            DrawRectangle(150, 250, 400, 35, Color.Yellow);
        }
        // TODO: Add your drawing code here
        _spriteBatch!.Begin();
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
