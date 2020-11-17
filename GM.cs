/* This simple project is my second adaptation from OneLoneCoder's YouTube video,
 * in which he created a 3D engine in C++, using only ASCII at the Windows Command Prompt,
 * to make a Wolfenstien style FPS game.
 * https://github.com/OneLoneCoder/CommandLineFPS
 * https://www.youtube.com/c/javidx9/
 * https://youtu.be/xW8skO7MFYw
 * 
 * 
 * I have adapted it to C#, and I'm using the Unity engine.
 * However this time, the philosphy of the project remains true to OLC's 3D engine. 
 * Which I believe is to say: given a completely 2D display, or a grid based display,
 * objects of some sort would act as pixels (I'm using gameobjects with small square white sprites),
 * map out the 3D world around you via vectors, and then draw the screen every frame,
 * performing calculations based on positions, angles, distances etc.
 * 
 * This is my first project really stepping out side of the unity engine as much as I can.
 * This is also my first time experimenting using multiple classes within a single component.
 * 
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Wolfen_Trwo_Dee
{/* As in 'truly two dimensional' */

    public class GM : MonoBehaviour
    {/* The Game Master */

        /* A few variables for an FPS display*/
        public Text fps;
        Canvas canvas;
        float time1, time2;

        void Start()
        {
            /* Set up the Camera. */
            Camera cam = gameObject.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 44;
            cam.farClipPlane = 10;
            cam.backgroundColor = Color.grey;
            transform.position = new Vector3(TrwoDeeEngine.nScreenWidth * .5f, TrwoDeeEngine.nScreenHeight * .5f, -10);
            cam.transform.Rotate(new Vector3(0, 0, 180));

            /* Set up the FPS display */
            fps = new GameObject("FPS Text").AddComponent<Text>();
            canvas = fps.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fps.transform.SetParent(gameObject.transform);
            fps.transform.position = gameObject.transform.position;
            fps.transform.Translate(0, 0, 5);
            Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            fps.alignment = TextAnchor.UpperLeft;
            fps.resizeTextForBestFit = false;
            fps.fontSize = 12;
            fps.font = ArialFont;
            fps.material = ArialFont.material;

            /* Continue on to set up the map */
            TrwoDeeEngine.SetUpMap();
        }

        void Update()
        {
            /* Update/calculate the FPS.
             * Check for user input, and control the player character.
             * Call the TrwoDeeEngineUpdate every frame. */

            fps.text = FPS();
            TrwoDeeEngine.UserInput();
            TrwoDeeEngine.TrwoDeeEngineUpdate();
        }

        string FPS()
        {
            time1 = Time.time;
            int fpsn = (int)(1 / (time1 - time2));
            time2 = time1;
            return fpsn.ToString();
        }
    }




    public class TrwoDeeEngine
    {
        /* Set the 'pixel' resolution and the map size. */
        public static int nScreenWidth = 160;
        public static int nScreenHeight = 90;
        public static int nMapHeight = 32;
        public static int nMapWidth = 64;

        /* Give the player some variables to controll movement etc. */
        public static float fPlayerX = 8;/* Players X position. */
        public static float fPlayerY = 8;/* Players Y position. */
        public static float fPlayerA = 0;/* Direction the player is facing */
        public static float fFOV = 1f; /* Players field-of-view. */
        public static float fDepth = Mathf.Sqrt(nMapWidth * nMapWidth) + (nMapHeight * nMapHeight); /* Default max ray distance. */
        public static float fPlayerMoveSpeed = 7.5f; /* Players movement speed. */
        public static float fPlayerTurnSpeed = 3.5f; /* Players turn speed. */

        /* The map. */
        public static string map1;

        /* The sprite to be used as screen pixles. */
        public static Sprite sprite;

        /* Cycle through this list of sprite renderers to update the 'pixels' every frame update. */
        public static List<SpriteRenderer> gridSR = new List<SpriteRenderer>();

        /* A parent object to parent the sprite-pixel game object,
         * and to keep the hierarchy clean. */
        public static GameObject board;

        public static void SetUpMap()
        {
            /* Load in the sprite asset */
            sprite = Resources.Load<Sprite>("ART/white");

            /* A programmer friendly visual repressentation of the map.
             * May be easily eddited, but if the X or Y change bounds, 
             * make sure to adjust nMapWidth & nMapHeight accordingly.
             * Also note that the grid X&Y is skewed, it looks taller than it should. */
            /////////0123456789ABCDEF1123456789ABCDEF2123456789ABCDEF3123456789ABCDEF
            map1 += "################################################################";//0
            map1 += "#..............................................................#";//1
            map1 += "#..............................................................#";//2
            map1 += "#..............................................................#";//3
            map1 += "#..............................................................#";//4
            map1 += "#..............................................................#";//5
            map1 += "#..............................................................#";//6
            map1 += "#...............................#..............................#";//7
            map1 += "#..............................................................#";//8
            map1 += "#..............................................................#";//9
            map1 += "#..............................................................#";//A
            map1 += "#..............................................................#";//B
            map1 += "#..............................................................#";//C
            map1 += "#..............................................................#";//D
            map1 += "#..............................................................#";//E
            map1 += "#.......#.......#.......#.......#.......#.......#.......#......#";//F
            map1 += "#..............................................................#";//1
            map1 += "#..............................................................#";//1
            map1 += "#..............................................................#";//2
            map1 += "#..............................................................#";//3
            map1 += "#..............................................................#";//4
            map1 += "#..............................................................#";//5
            map1 += "#..............................................................#";//6
            map1 += "#...............................#..............................#";//7
            map1 += "#..............................................................#";//8
            map1 += "#..............................................................#";//9
            map1 += "#..............................................................#";//A
            map1 += "#..............................................................#";//B
            map1 += "#..............................................................#";//C
            map1 += "#..............................................................#";//D
            map1 += "#..............................................................#";//E
            map1 += "################################################################";//F

            /* Create the board. */
            board = new GameObject("Board");

            for (int x = 0; x < nScreenWidth; x++)
            {
                for (int y = 0; y < nScreenHeight; y++)
                {/* Instantiate, designate, and position the 'pixels' sprite-gameobjects, 
                  * then add them to the list. */
                    GameObject g = new GameObject("Grid X:" + x + ", Y: " + y);
                    g.transform.SetParent(board.transform);
                    g.transform.position = new Vector3(x, y, 0);
                    SpriteRenderer gs = g.AddComponent<SpriteRenderer>();
                    gs.sprite = sprite;
                    gridSR.Add(gs);
                }
            }

        }

        public static void UserInput()
        {
            if (Input.anyKey)
            {
                if (Input.GetKey(KeyCode.A))
                {
                    fPlayerA += Time.deltaTime * fPlayerTurnSpeed;
                }

                if (Input.GetKey(KeyCode.D))
                {
                    fPlayerA -= Time.deltaTime * fPlayerTurnSpeed;
                }

                if (Input.GetKey(KeyCode.W))
                {
                    fPlayerX += Mathf.Sin(fPlayerA) * fPlayerMoveSpeed * Time.deltaTime;
                    fPlayerY += Mathf.Cos(fPlayerA) * fPlayerMoveSpeed * Time.deltaTime;

                    /* Check the player position for walls. */
                    if (map1[(int)fPlayerY * nMapWidth + (int)fPlayerX] == '#')
                    {/* If the player hits, move him back. */
                        fPlayerX -= Mathf.Sin(fPlayerA) * fPlayerMoveSpeed * Time.deltaTime;
                        fPlayerY -= Mathf.Cos(fPlayerA) * fPlayerMoveSpeed * Time.deltaTime;
                    }
                }

                if (Input.GetKey(KeyCode.S))
                {
                    fPlayerX -= Mathf.Sin(fPlayerA) * fPlayerMoveSpeed * Time.deltaTime;
                    fPlayerY -= Mathf.Cos(fPlayerA) * fPlayerMoveSpeed * Time.deltaTime;

                    /* Check the player position for walls. */
                    if (map1[(int)fPlayerY * nMapWidth + (int)fPlayerX] == '#')
                    {/* If the player hits, move him back. */
                        fPlayerX += Mathf.Sin(fPlayerA) * fPlayerMoveSpeed * Time.deltaTime;
                        fPlayerY += Mathf.Cos(fPlayerA) * fPlayerMoveSpeed * Time.deltaTime;
                    }
                }
            }
        }

        public static void TrwoDeeEngineUpdate()
        {
            for (int x = 0; x < nScreenWidth; x++)
            {
                /* For each pixel collumn(the x coordinant),
                 * calculate the ray projection from the player, within the players FOV.
                 * 
                 * Split the players FOV from the players facing direction, one ray for every column,
                 * and for each ray, check the distance to any wall at that angle. */
                float fRayAngle = fPlayerA - fFOV / 2f + x / (float)nScreenWidth * fFOV;

                /* Start distance at 0, and incrimentally check to find the walls distance. */
                float fDistToWall = 0;
                bool bHitWall = false;
                bool bBoundary = false;

                /* Unit Vector for ray in player space, 
                 * ie. the ray angle in relationship to the players direction. */
                float fEyeX = Mathf.Sin(fRayAngle);
                float fEyeY = Mathf.Cos(fRayAngle);

                while (!bHitWall && fDistToWall < fDepth)
                {/* Start testing distances. */

                    /* Distance incriment */
                    fDistToWall += .1f;

                    /* Set new distance to players facing direciton. */
                    int nTestX = (int)(fPlayerX + fEyeX * fDistToWall);
                    int nTestY = (int)(fPlayerY + fEyeY * fDistToWall);

                    /* Test if ray is OB(out of bounds) */
                    if (nTestX < 0 || nTestX > nMapWidth || nTestY < 0 || nTestY > nMapHeight)
                    {
                        /* When it is OB, flag wall is true, and set the default depth.
                         * Essentially saying: there is a wall, but it's so far away it will be in the dark. 
                         * Also prevents walls from shrinking to nothing. */
                        bHitWall = true;
                        fDistToWall = fDepth;
                    }

                    else
                    {/* Else the ray must be inbounds, 
                      
                      * test to see if the ray-cell is a wall-block. */
                        if (map1[nTestY * nMapWidth + nTestX] == '#')
                        {
                            bHitWall = true;

                            /* The next block is to find and color the corners of the walls,
                             * which helps in creating the 3D-illusion of perspective.
                             * 
                             * Create an array of floats for distances(or magnitudes) and for dots. */
                            float[] faDist = new float[4];
                            float[] faDot = new float[4];

                            for (int tX = 0; tX < 2; tX++)
                            {
                                for (int tY = 0; tY < 2; tY++)
                                {
                                    /* Find distances and dots, and populate the arrays. */
                                    float vecY = nTestY + tY - fPlayerY;
                                    float vecX = nTestX + tX - fPlayerX;
                                    float fDist = Mathf.Sqrt(vecX * vecX + vecY * vecY);
                                    float fDot = (fEyeX * vecX / fDist) + (fEyeY * vecY / fDist);

                                    faDist[tX * 2 + tY] = fDist;
                                    faDot[tX * 2 + tY] = fDot;
                                }
                            }

                            /* How much of the wall we want to be considered 'Corner'. */
                            float fBound = .003f;

                            /* Sort the corners by distance. */
                            Array.Sort(faDist, faDot);

                            /* Check for the 2 closest corners. */
                            if (Mathf.Acos(faDot[0]) < fBound) { bBoundary = true; }
                            if (Mathf.Acos(faDot[1]) < fBound) { bBoundary = true; }
                        }
                    }
                }

                /* Calculate disctances to ceiling and to floor. */
                int nCeiling = (int)((nScreenHeight / 2) - nScreenHeight / ((float)fDistToWall));
                int nFloor = nScreenHeight - nCeiling;

                for (int y = 0; y < nScreenHeight; y++)
                { /* Ready to color the 'pixels'! */
                    if (y + 25 <= nCeiling)
                    {/* Designate as a ceiling pixel, and color/gradient accordingly. */
                        float r = .45f - (y * y * .0008374f);
                        float g = .55f - (y * y * .0008395f);
                        float b = .55f - (y * y * .0008392f);

                        gridSR[x * nScreenHeight + y].color = new Color(r, g, b, 1);
                    }

                    else if (y + 25 >= nCeiling && y + 0 <= +nFloor)
                    {/* Designate as a wall pixel, and color/gradient accordingly
                      * Also change the height of the walls. */
                        float r = .75f - (fDistToWall * .024512f);
                        float g = .85f - (fDistToWall * .024517f);
                        float b = .75f - (fDistToWall * .024517f);

                        gridSR[x * nScreenHeight + y].color = new Color(r, g, b, 1);

                        if (bBoundary)
                        {/* Designate as a wall-corner pixel, and color/gradient accordingly. */
                            r *= .5f;
                            g *= .5f;
                            b *= .5f;
                            gridSR[x * nScreenHeight + y].color = new Color(r, g, b, 1);
                        }
                    }

                    else
                    {/* Designate as a floor pixel, and color/gradient accordingly. */
                        float r = .000075f + (y * y * y * .00000029894f);
                        float g = .000075f + (y * y * y * .00000029895f);
                        float b = .000065f + (y * y * y * .00000028872f);
                        gridSR[x * nScreenHeight + y].color = new Color(r, g, b, 1);
                    }
                }
            }
        }
    }
}