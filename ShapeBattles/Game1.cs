
#region Using Statements
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using XNAExtras;
#endregion

namespace ShapeBattles
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        ContentManager content;
        SpriteBatch spriteBatch;
        Random rand = new Random();
        Player player;
        ArrayList shots = new ArrayList();
        ArrayList enemies = new ArrayList();
        ArrayList debris = new ArrayList();
        BitmapFont m_font;
        AudioEngine audioEngine;
        SoundBank soundBank;
        WaveBank waveBank;
        Cue gunsound = null;
    
        private int holdElapsedTime;
        private int holdPlayerLives;
        private float ammoSize;
        private int timeBetweenShots;
        private double sinceLastShot;
        private double playerDeathTime;
        private Texture2D background;
        private Texture2D gameover;
        private Texture2D superbombs;
        bool RShoulderPressed;

        int currentDifficultyLevel;     //difficulty level
        int betweenWaves;               //time (seconds) between waves of attack
        int waveAmount;                 //number of enemies per wave

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            content = new ContentManager(Services);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            m_font = new BitmapFont("Content/Font/comic.xml");
            audioEngine = new AudioEngine("Content\\Assets\\GameAudio.xgs");
            soundBank = new SoundBank(audioEngine, "Content\\Assets\\Sound Bank.xsb");
            waveBank = new WaveBank(audioEngine, "Content\\Assets\\Wave Bank.xwb");

            base.Initialize();
        }

        /// <summary>
        /// Load your graphics content.  If loadAllContent is true, you should
        /// load content from both ResourceManagementMode pools.  Otherwise, just
        /// load ResourceManagementMode.Manual content.
        /// </summary>
        /// <param name="loadAllContent">Which type of content to load.</param>
        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            if (loadAllContent)
            {
                // TODO: Load any ResourceManagementMode.Automatic content
                Viewport viewport = graphics.GraphicsDevice.Viewport;
                Player.GraphicsViewport = viewport;
                Projectile.GraphicsViewport = viewport;
                Enemy.GraphicsViewport = viewport;

                spriteBatch = new SpriteBatch(this.graphics.GraphicsDevice);
                
                background = content.Load<Texture2D>("Content\\Assets\\background");
                gameover = content.Load<Texture2D>("Content\\Assets\\gameover");
                superbombs = content.Load<Texture2D>("Content\\Assets\\superbomb");
                Player.Texture = content.Load<Texture2D>("Content\\Assets\\shuttle2");
                Projectile.Texture = content.Load<Texture2D>("Content\\Assets\\projectile");
                
                Enemy.rand = new Random();

                StartGame();
            }

            // TODO: Load any ResourceManagementMode.Manual content
            m_font.Reset(graphics.GraphicsDevice);
        }

        /// <summary>
        /// Unload your graphics content.  If unloadAllContent is true, you should
        /// unload content from both ResourceManagementMode pools.  Otherwise, just
        /// unload ResourceManagementMode.Manual content.  Manual content will get
        /// Disposed by the GraphicsDevice during a Reset.
        /// </summary>
        /// <param name="unloadAllContent">Which type of content to unload.</param>
        protected override void UnloadGraphicsContent(bool unloadAllContent)
        {
            if (unloadAllContent == true)
            {
                content.Unload();
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>       
        protected override void Update(GameTime gameTime)
        {
            // Allows the default game to exit on Xbox 360 and Windows
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            audioEngine.Update();

            //if the player is still ALIVE then perform all game logic
            if (player.Status)  
            {
                player.UpdateMovement(GamePad.GetState(PlayerIndex.One));

                //create new enemies every "wave" (starts out slow and gets faster and faster)
                int elapsedTime = gameTime.TotalGameTime.Seconds;
                if (elapsedTime % betweenWaves == 0 && elapsedTime > 0 && elapsedTime != holdElapsedTime && playerDeathTime == 0)
                {
                    NextWaveofBadGuys(elapsedTime);
                }

                //move the enemies and any debris that exist from recently destroyed enemies
                MoveEnemies();
                MoveDebris();

                //check for player input and handle accordingly
                CheckControllerInput(gameTime);

                //move and check for collisions all projectiles currently being fired
                HandleProjectiles();                
            }
            else
            {
                //player is DEAD! (Allow them to restart by pressing START)
                if (gunsound != null && gunsound.IsPlaying)
                {
                    gunsound.Pause();
                }
                if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed)
                {
                    StartGame();
                }
            }

            //make sure player didnt die this cycle, if so handle appropriately
            CheckPlayerDeath(gameTime);
            
            base.Update(gameTime);
        }
        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            
            //first draw our background, since it should be the bottom most texture (otherwise we wouldnt seen our other sprites)
            spriteBatch.Draw(background, new Vector2(0.0f, 0.0f), Color.White);
            
            //dont show player model for 1 second when death occurs
            if ((Math.Abs(gameTime.TotalGameTime.TotalSeconds) - Math.Abs(playerDeathTime) > 2) || playerDeathTime == 0)
            {
                player.Draw(spriteBatch);
                playerDeathTime = 0;
            }
            
            //display each Projectile in the ArrayList
            foreach (Projectile cshot in shots)
            {
                cshot.Draw(spriteBatch);
            }

            //display each Enemy in the ArrayList
            foreach (Enemy cenemy in enemies)
            {
                cenemy.Draw(spriteBatch);
            }

            //display each Debris in the ArrayList (basically they are tiny enemies that dont cause damage)
            foreach (Enemy cdebris in debris)
            {
                cdebris.Draw(spriteBatch);
            }

            //if the player is DEAD, draw the Game Over screen
            if (player.Status == false)
                spriteBatch.Draw(gameover, new Vector2(240f, 260f), Color.White);

            //show SuperBomb and Player lives remaining sprites
            spriteBatch.Draw(superbombs, new Vector2(750f, 10f), Color.White);
            spriteBatch.Draw(Player.Texture, new Vector2(680f, 8f), Color.White);
            
            spriteBatch.End();

            //using XNAExtras, draw fonts for Score, Player Lives, and MaxBombs
            m_font.DrawString(10, 3, Color.White, "Score: " + player.Score.ToString());
            if (player.Multiplier > 1)
                m_font.DrawString(150, 3, Color.Red, player.Multiplier + "x!");
            m_font.DrawString(710, 3, Color.White, "x " + player.Lives.ToString());
            m_font.DrawString(770, 3, Color.Orange, "x " + player.MaxBombs.ToString());

            base.Draw(gameTime);
        }

        /// <summary>
        /// This is called when the game is first started, and every time the player Restarts (after killing off all of their lives)
        /// </summary>
        private void StartGame()
        {
            //Beginning of Game or after Player dies and wants to play again:
            currentDifficultyLevel = 1;
            betweenWaves = 5;
            waveAmount = 10;
            holdElapsedTime = 0;
            sinceLastShot = 0;
            timeBetweenShots = 100;
            ammoSize = 1.0f;
            RShoulderPressed = false;
            playerDeathTime = 0;
            player = new Player();
            holdPlayerLives = player.Lives;
            shots.Clear();
            enemies.Clear();
            debris.Clear();

            //start with 5 Enemies to begin with
            for (int x = 0; x < waveAmount; x++)
                enemies.Add(new Enemy(content));
        }

        /// <summary>
        /// This method increases the Difficulty on each new wave of enemies and adds new enemies based on the current "waveAmount"
        /// (which is steadily increased in IncreaseDifficulty()
        /// </summary>
        /// <param name="elapsedTime"></param>
        private void NextWaveofBadGuys(int elapsedTime)
        {
            IncreaseDifficulty();

            //add waveAmount number of new enemies for this wave of attack
            for (int j = 1; j < waveAmount; j++)
                enemies.Add(new Enemy(content));

            //store elapsedTime for comparison next cycle
            holdElapsedTime = elapsedTime;
        }

        /// <summary>
        /// This slowly increases the game difficulty the longer the player is alive by increase waves of attack timing and amount
        /// Bonus: player also gets improved weapon and score modifier
        /// As difficulty increases, the chance that the players ammo could randomly be set to a color of one of the enemies is introduced
        /// This means that whatever color ammo player is shooting will only destroy enemies of the same color (this is for a very short period, and only to keep players on their toes)
        /// Enemy.BaddassRandomness = As the game gets more difficult, the chances of a Purple enemy (very fast) spawning gets greater and greater
        /// </summary>
        private void IncreaseDifficulty()
        {
            currentDifficultyLevel++;
            player.AmmoColor = "white";
            ammoSize = 1.0f;

            if (currentDifficultyLevel == 40)
            {
                waveAmount = 40;
                betweenWaves = 1;
                Enemy.BadassRandomness = 50;
                player.Multiplier++;
                soundBank.PlayCue("reward");
                timeBetweenShots = 20;

                int randomColorAmmo = rand.Next(10, 14);
                if (randomColorAmmo == 10)
                    player.AmmoColor = "red";
                else if (randomColorAmmo == 11)
                    player.AmmoColor = "yellow";
                else if (randomColorAmmo == 12)
                    player.AmmoColor = "blue";
                else if (randomColorAmmo == 13)
                    player.AmmoColor = "green";
                else
                    player.AmmoColor = "white";
            }
            else if (currentDifficultyLevel == 20)
            {
                waveAmount = 30;
                betweenWaves = 2;
                Enemy.BadassRandomness = 60;
                player.Multiplier++;
                soundBank.PlayCue("reward");
                timeBetweenShots = 20;

                int randomColorAmmo = rand.Next(10, 14);
                if (randomColorAmmo == 10)
                    player.AmmoColor = "red";
                else if (randomColorAmmo == 11)
                    player.AmmoColor = "yellow";
                else if (randomColorAmmo == 12)
                    player.AmmoColor = "blue";
                else if (randomColorAmmo == 13)
                    player.AmmoColor = "green";
                else
                    player.AmmoColor = "white";
            }
            else if (currentDifficultyLevel == 15)
            {
                waveAmount = 25;
                betweenWaves = 3;
                Enemy.BadassRandomness = 70;
                player.Multiplier++;
                soundBank.PlayCue("reward");
                timeBetweenShots = 30;
                int randomColorAmmo = rand.Next(10, 30);
                if (randomColorAmmo == 10)
                    player.AmmoColor = "red";
                else if (randomColorAmmo == 11)
                    player.AmmoColor = "yellow";
                else if (randomColorAmmo == 12)
                    player.AmmoColor = "blue";
                else if (randomColorAmmo == 13)
                    player.AmmoColor = "green";
                else
                    player.AmmoColor = "white";
            }
            else if (currentDifficultyLevel == 10)
            {
                waveAmount = 20;
                betweenWaves = 4;
                Enemy.BadassRandomness = 80;
                player.Multiplier++;
                soundBank.PlayCue("reward");
                timeBetweenShots = 30;
                if (player.ShotsFired == 0)
                    player.Score += 5000f;  //highly unlikely
            }
            else if (currentDifficultyLevel == 5)
            {
                waveAmount = 15;
                betweenWaves = 4;
                Enemy.BadassRandomness = 90;
                player.Multiplier++;
                soundBank.PlayCue("reward");
                timeBetweenShots = 50;
                if (player.ShotsFired == 0)
                    player.Score += 1000f;  //pacification bonus! (survived this long without firing 1 shot)
            }


            //If you wanted to mess around with randomly increase projectile size and speed (like Geometry Wars) this would work:
            //ammoSize = 2.5f;              //larger bullets
            //timeBetweenShots = 150;       //slower
            //player.MaxShots = 10;         //less of them

        }

        /// <summary>
        /// This checks to see if the Right Stick is moved (to fire) and if the Right Bumper was pressed (to drop MegaBomb!)
        /// A tiny beep (wav) is played if a shot is fired
        /// </summary>
        private void CheckControllerInput(GameTime gameTime)
        {
            Vector2 holdLeftStick = new Vector2(0.0f, 0.0f);
            Projectile curShot;

            if ((GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.X != 0 || GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.Y != 0) && shots.Count < player.MaxShots && gameTime.TotalGameTime.TotalMilliseconds - sinceLastShot > timeBetweenShots && playerDeathTime == 0)
            {
                float angle = 0.0f;
                Vector2 rightStick = GamePad.GetState(PlayerIndex.One).ThumbSticks.Right;
                rightStick.Normalize();
                if ((rightStick.X >= 0 || rightStick.X <= 0) || (rightStick.Y >= 0 || rightStick.Y <= 0))
                {
                    angle = (float)Math.Acos(rightStick.Y);
                    if (rightStick.X < 0.0f)
                        angle = -angle;
                }
                sinceLastShot = gameTime.TotalGameTime.TotalMilliseconds;
                shots.Add(new Projectile(player.Position, new Vector2(rightStick.X * 10, -rightStick.Y * 10), player.AmmoColor, ammoSize));
                player.ShotsFired++;
                curShot = (Projectile)shots[shots.Count - 1];
                curShot.Angle = angle;

                soundBank.PlayCue("shot2");
            }
            else
            {
                if (gunsound != null && gunsound.IsPlaying)
                {
                    gunsound.Pause();
                }
            }
            //this is the MEGA BOMB - kills every enemy on screen
            if (GamePad.GetState(PlayerIndex.One).Buttons.RightShoulder == ButtonState.Pressed && !RShoulderPressed && player.MaxBombs > 0)
            {
                //make sure only 1 press of button is registered (so all bombs arent dropped instantly)
                RShoulderPressed = true;
                DropTheBomb();
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.RightShoulder == ButtonState.Released && RShoulderPressed)
                RShoulderPressed = false;
        }

        /// <summary>
        /// This loops through every shot in the ArrayList and checks to see if contact with an enemy occurred.
        /// If so KillEnemy is called passing the current enemy that needs to be destroyed
        /// </summary>
        private void HandleProjectiles()
        {
            Projectile curShot;
            for (int x = 0; x < shots.Count; x++)
            {
                curShot = (Projectile)shots[x];
                curShot.UpdateMovement();
                //TO DO: loop through all Enemies and check collision against each
                for (int y = 0; y < enemies.Count; y++)
                {
                    Enemy cEnemy = (Enemy)enemies[y];
                    if (cEnemy.Living && cEnemy.Damages)
                        curShot.CheckCollision(cEnemy);

                    //if the Enemy died as the result of a collision with a shot, then remove them from the ArrayList (they no longer exist)
                    if (!cEnemy.Living && cEnemy.Damages)
                    {
                        KillEnemy(cEnemy.sColor, cEnemy.Position, y);
                        //since we are screwing with the total Count of enemies, restart our loop so we dont miss anyone
                        y = -1;
                    }
                }
                //end loop

                //if the shot hit something or left the stage - it has been "fired" and is no longer needed
                if (curShot.ShotFired)
                    shots.RemoveAt(x);
            }
        }

        /// <summary>
        /// This uses one of the players megaBombs to destroy every enemy on the screen.  
        /// It adds 1 projectile (off screen), flags every currently living enemy as no longer living, and deducts 1 from the players maxBombs property
        /// </summary>
        private void DropTheBomb()
        {
            foreach (Enemy cEnemy in enemies)
            {
                //Enemy cEnemy = enemies[y];
                if (cEnemy.Living && cEnemy.Damages)
                    cEnemy.Living = false;
            }

            shots.Add(new Projectile(new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f), "white", 0.0f));
            Projectile bigBomb = (Projectile)shots[shots.Count - 1];
            player.ShotsFired = player.ShotsFired + 1;
            bigBomb.ShotFired = true;
            bigBomb.IsBigBomb = true;

            if (player.MaxBombs > 0 && holdPlayerLives == player.Lives)
                player.MaxBombs = player.MaxBombs - 1;
        }

        /// <summary>
        /// This loops through every Enemy in the ArrayList and calls its UpdateMovement function (which moves the sprite)
        /// (making sure to ONLY move the sprite if they player hasnt been killed)
        /// </summary>
        private void MoveEnemies()
        {
            Enemy curEnemy;
            for (int x = 0; x < enemies.Count; x++)
            {
                curEnemy = (Enemy)enemies[x];
                if (holdPlayerLives == player.Lives)
                    curEnemy.UpdateMovement(player);
            }            
        }

        /// <summary>
        ///  This loops through every Debris in the ArrayList and calls its UpdateMovement function (which moves the sprite)
        /// </summary>
        private void MoveDebris()
        {
            Enemy curDebris;
            for (int x = 0; x < debris.Count; x++)
            {
                curDebris = (Enemy)debris[x];
                curDebris.UpdateMovement(player);
            }

            //clean up! - make sure all debris that are now offscreen are removed from the ArrayList (once offscreen they are no longer "living")
            Enemy curEnemy;
            int e = 0;
            while (e < debris.Count)
            {
                curEnemy = (Enemy)debris[e];
                if (!curEnemy.Living && !curEnemy.Damages)
                    debris.RemoveAt(e);
                e++;
            }
        }

        private void CheckPlayerDeath(GameTime gameTime)
        {
            //if player.Lives is not the same as the last cycle, its because hes been killed
            if (holdPlayerLives != player.Lives)
            {
                //as long as the player still has lives to be spared, reset the playing field and allow him to resume
                if (player.Lives > 0)
                {
                    //if the player dies, kill every currently living enemy so when he respawns he has a fair chance
                    DropTheBomb();
                    KillPlayer(player.Position);
                    playerDeathTime = gameTime.TotalGameTime.TotalSeconds;
                }
                else
                    player.Status = false;
            }

            //restore playerLives every cycle to make sure an enemy doesn't kill the player more than once (or if 2 enemies hit player at the same time)
            holdPlayerLives = player.Lives;
        }

        /// <summary>
        /// Removes the enemy from the ArrayList and adds multiple new enmies slightly smaller to a Debris ArrayList (chunks of the destoryed enemy) that fly off in a star pattern
        /// </summary>
        /// <param name="sColor">Color of the destroyed enemy (used when spawning new debris parts that should be same color)</param>
        /// <param name="position">Originating position of the "explosion" (all parts start here and work their way towards the edge of the screen)</param>
        /// <param name="y">The index for the enemy that has just been killed (so it can be removed from the ArrayList)</param>
        private void KillEnemy(string sColor, Vector2 position, int y)
        {
            //debris is just tiny enemies that dont cause damage (_damage = false) so they cant hurt the player
            debris.Add(new Enemy(content, false, sColor, position, new Vector2(0.0f, -8.0f)));  //up
            debris.Add(new Enemy(content, false, sColor, position, new Vector2(7.0f, -7.0f)));  //up/right
            debris.Add(new Enemy(content, false, sColor, position, new Vector2(8.0f, 0.0f)));   //right
            debris.Add(new Enemy(content, false, sColor, position, new Vector2(7.0f, 7.0f)));   //right/down
            debris.Add(new Enemy(content, false, sColor, position, new Vector2(0.0f, 8.0f)));   //down
            debris.Add(new Enemy(content, false, sColor, position, new Vector2(-7.0f, -7.0f)));   //left/down
            debris.Add(new Enemy(content, false, sColor, position, new Vector2(-8.0f, 0.0f)));  //left
            debris.Add(new Enemy(content, false, sColor, position, new Vector2(-7.0f, 7.0f)));  //left/up
            enemies.RemoveAt(y);    //finally remove the original Enemy from existing
            soundBank.PlayCue("explode");
            switch (sColor)
            {
                case "yellow":
                    player.Score += 10f * player.Multiplier;
                    break;
                case "blue":
                    player.Score += 10f * player.Multiplier;
                    break;
                case "green":
                    player.Score += 5f * player.Multiplier;
                    break;
                case "red":
                    player.Score += 20f * player.Multiplier;
                    break;
                case "purple":
                    player.Score += 50f * player.Multiplier;
                    break;
            }
        }

        /// <summary>
        /// Similar to KillEnemy above, except this is just for the player (because he was hit by an enemy)
        /// </summary>
        /// <param name="position">Originating position of the "explosion" (all parts start here and work their way towards the edge of the screen)</param>
        private void KillPlayer(Vector2 position)
        {
            debris.Add(new Enemy(content, false, "white", position, new Vector2(0.0f, -8.0f)));  //up
            debris.Add(new Enemy(content, false, "white", position, new Vector2(7.0f, -7.0f)));  //up/right
            debris.Add(new Enemy(content, false, "white", position, new Vector2(8.0f, 0.0f)));   //right
            debris.Add(new Enemy(content, false, "white", position, new Vector2(7.0f, 7.0f)));   //right/down
            debris.Add(new Enemy(content, false, "white", position, new Vector2(0.0f, 8.0f)));   //down
            debris.Add(new Enemy(content, false, "white", position, new Vector2(-7.0f, -7.0f)));   //left/down
            debris.Add(new Enemy(content, false, "white", position, new Vector2(-8.0f, 0.0f)));  //left
            debris.Add(new Enemy(content, false, "white", position, new Vector2(-7.0f, 7.0f)));  //left/up
            soundBank.PlayCue("player_explode");
            player.Position = new Vector2(390f, 290f);  //recenter the player after they died
            player.Multiplier = 1;
            player.MaxShots = 35;
            timeBetweenShots = 100;
        }
    }
}