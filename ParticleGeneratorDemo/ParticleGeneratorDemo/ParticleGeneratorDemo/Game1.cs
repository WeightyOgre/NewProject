using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ParticleGeneratorDemo
{

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }

        // Used to draw the instructions on the screen.
        SpriteFont font;

        // a random number generator that the whole sample can share.
        private static Random random = new Random();
        public static Random Random
        {
            get { return random; }
        }

        // Here's the really fun part of the sample, the particle systems! These are
        // drawable game components, so we can just add them to the components
        // collection. Read more about each particle system in their respective source
        // files.
        ExplosionParticleSystem explosion;
        ExplosionSmokeParticleSystem smoke;
        SmokePlumeParticleSystem smokePlume;

        // State is an enum that represents which effect we're currently demoing.
        enum State
        {
            Explosions,
            SmokePlume
        };

        // the number of values in the "State" enum.
        const int NumStates = 2;
        State currentState = State.Explosions;

        // a timer that will tell us when it's time to trigger another explosion.
        const float TimeBetweenExplosions = 2.0f;
        float timeTillExplosion = 0.0f;

        // keep a timer that will tell us when it's time to add more particles to the
        // smoke plume.
        const float TimeBetweenSmokePlumePuffs = .5f;
        float timeTillPuff = 0.0f;

        // keep track of the last frame's keyboard and gamepad state, so that we know
        // if the user has pressed a button.
        KeyboardState lastKeyboardState;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            // create the particle systems and add them to the components list.
            // we should never see more than one explosion at once
            explosion = new ExplosionParticleSystem(this, 1);
            Components.Add(explosion);

            // but the smoke from the explosion lingers a while.
            smoke = new ExplosionSmokeParticleSystem(this, 2);
            Components.Add(smoke);

            // we'll see lots of these effects at once; this is ok
            // because they have a fairly small number of particles per effect.
            smokePlume = new SmokePlumeParticleSystem(this, 9);
            Components.Add(smokePlume);
            
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            font = Content.Load<SpriteFont>("font");
        }

        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here
            // check the input devices to see if someone has decided they want to see
            // the other effect, if they want to quit.
            HandleInput();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            switch (currentState)
            {
                // if we should be demoing the explosions effect, check to see if it's
                // time for a new explosion.
                case State.Explosions:
                    UpdateExplosions(dt);
                    break;
                // if we're showing off the smoke plume, check to see if it's time for a
                // new puff of smoke.
                case State.SmokePlume:
                    UpdateSmokePlume(dt);
                    break;
            }
            base.Update(gameTime);
        }

        // this function is called when we want to demo the smoke plume effect. it
        // updates the timeTillPuff timer, and adds more particles to the plume when
        // necessary.
        private void UpdateSmokePlume(float dt)
        {
            timeTillPuff -= dt;
            if (timeTillPuff < 0)
            {
                Vector2 where = Vector2.Zero;
                // add more particles at the bottom of the screen, halfway across.
                where.X = graphics.GraphicsDevice.Viewport.Width / 2;
                where.Y = graphics.GraphicsDevice.Viewport.Height;
                smokePlume.AddParticles(where);

                // and then reset the timer.
                timeTillPuff = TimeBetweenSmokePlumePuffs;
            }
        }

        // this function is called when we want to demo the explosion effect. it
        // updates the timeTillExplosion timer, and starts another explosion effect
        // when the timer reaches zero.
        private void UpdateExplosions(float dt)
        {
            timeTillExplosion -= dt;
            if (timeTillExplosion < 0)
            {
                Vector2 where = Vector2.Zero;
                // create the explosion at some random point on the screen.
                where.X = RandomBetween(0, graphics.GraphicsDevice.Viewport.Width);
                where.Y = RandomBetween(0, graphics.GraphicsDevice.Viewport.Height);

                // the overall explosion effect is actually comprised of two particle
                // systems: the fiery bit, and the smoke behind it. add particles to
                // both of those systems.
                explosion.AddParticles(where);
                smoke.AddParticles(where);

                // reset the timer.
                timeTillExplosion = TimeBetweenExplosions;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            // draw some instructions on the screen
            string message = string.Format("Current effect: {0}!\n" +
                "Hit the A button or space bar, or tap the screen, to switch.\n\n" +
                "Free particles:\n" +
                "    ExplosionParticleSystem:      {1}\n" +
                "    ExplosionSmokeParticleSystem: {2}\n" +
                "    SmokePlumeParticleSystem:     {3}",
                currentState, explosion.FreeParticleCount,
                smoke.FreeParticleCount, smokePlume.FreeParticleCount);
            spriteBatch.DrawString(font, message, new Vector2(50, 50), Color.White);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        // This function will check to see if the user has just pushed the A button or
        // the space bar. If so, we should go to the next effect.
        private void HandleInput()
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();
            GamePadState currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Allows the game to exit
            if (currentGamePadState.Buttons.Back == ButtonState.Pressed ||
                currentKeyboardState.IsKeyDown(Keys.Escape))
                this.Exit();


            // check to see if someone has just released the space bar.            
            bool keyboardSpace =
                currentKeyboardState.IsKeyUp(Keys.Space) &&
                lastKeyboardState.IsKeyDown(Keys.Space);


            // if either the A button or the space bar was just released, or the screen
            // was tapped, move to the next state. Doing modulus by the number of 
            // states lets us wrap back around to the first state.
            if (keyboardSpace)
            {
                currentState = (State)((int)(currentState + 1) % NumStates);
            }

            lastKeyboardState = currentKeyboardState;

        }

        //  a handy little function that gives a random float between two
        // values. This will be used in several places in the sample, in particilar in
        // ParticleSystem.InitializeParticle.
        public static float RandomBetween(float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

    }
}
