using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace SubService
{
    class Bluehack
    {
        internal static List<BHelper.PlayerDraw> PlayerDraws = new List<BHelper.PlayerDraw>();
        internal static List<BHelper.EntityDraw> EntityDraws = new List<BHelper.EntityDraw>();

        internal static int[,] BonePairsT = new int[,]
        {
            {6, 0},
            {73, 75},
            {71, 68},
            {10, 11},
            {38, 39},
            {71, 0},
            {73, 0},
            {39, 40},
            {11, 12},
            {38, 6},
            {10, 6}
        };
        internal static int[,] BonePairsCT = new int[,]
        {
            {6, 0},
            {70, 71},
            {82, 78},
            {40, 41},
            {11, 12},
            {82, 0},
            {70, 0},
            {12, 13},
            {41, 43},
            {11, 6},
            {40, 6}
        };

        static void ThreadESP()
        {
            while (true)
            {
                #region Player ESP
                for (int i = 0; i < 32; i++)
                {
                    bool IsAlive = BHelper.EntityList.GetLifeState(i);
                    int Base = BHelper.EntityList.Base(i);
                    if (IsAlive && Base != BHelper.LocalPlayer.Base() && Base != 0 && !BHelper.EntityList.GetDormant(i) && BHelper.EntityList.GetTeam(i) != BHelper.LocalPlayer.GetTeam())
                    {
                        float[] EnemyHeadPos = BHelper.EntityList.GetBonePosition(i, 8);
                        float[] PlayerPos = BHelper.LocalPlayer.GetPosition();
                        float[] ScreenPos = new float[2];

                        if (WorldToScreen(i, EnemyHeadPos, ref ScreenPos))
                        {
                            BHelper.PlayerDraw PlayerInfo = new BHelper.PlayerDraw();

                            PlayerInfo.PlayerBase = Base;
                            PlayerInfo.PlayerID = i;
                            PlayerInfo.PlayerTeam = BHelper.EntityList.GetTeam(i);
                            PlayerInfo.PlayerHealth = BHelper.EntityList.GetHealth(i);

                            PlayerInfo.espX = ScreenPos[0];
                            PlayerInfo.espY = ScreenPos[1];
                            PlayerInfo.espDistance = (float)Math.Sqrt((EnemyHeadPos[0] - PlayerPos[0]) * (EnemyHeadPos[0] - PlayerPos[0]) + (EnemyHeadPos[1] - PlayerPos[1]) * (EnemyHeadPos[1] - PlayerPos[1]) + (EnemyHeadPos[2] - PlayerPos[2]) * (EnemyHeadPos[2] - PlayerPos[2]));
                            PlayerInfo.Bones = new float[44];

                            int[,] BonePairs;
                            if (PlayerInfo.PlayerTeam == 3) BonePairs = BonePairsCT;
                            else BonePairs = BonePairsT;

                            for (int i_1 = 0; i_1 < 11; i_1++)
                            {
                                float[] EnemyBonePos1 = BHelper.EntityList.GetBonePosition(i, BonePairs[i_1, 0]);
                                float[] EnemyBonePos2 = BHelper.EntityList.GetBonePosition(i, BonePairs[i_1, 1]);
                                float[] ScreenPosBone1 = new float[2];
                                float[] ScreenPosBone2 = new float[2];

                                if (WorldToScreen(i, EnemyBonePos1, ref ScreenPosBone1) && WorldToScreen(i, EnemyBonePos2, ref ScreenPosBone2))
                                {
                                    PlayerInfo.Bones[(i_1 * 4) + 0] = ScreenPosBone1[0];
                                    PlayerInfo.Bones[(i_1 * 4) + 1] = ScreenPosBone1[1];
                                    PlayerInfo.Bones[(i_1 * 4) + 2] = ScreenPosBone2[0];
                                    PlayerInfo.Bones[(i_1 * 4) + 3] = ScreenPosBone2[1];
                                }
                            }

                            int PlayerIndex = FindPlayerDrawIndexByBase(PlayerDraws, Base);
                            if (PlayerIndex == -1)
                                PlayerDraws.Add(PlayerInfo);
                            else
                                PlayerDraws[PlayerIndex] = PlayerInfo;
                        }
                    }
                    else
                    {
                        PlayerDraws.Remove(FindPlayerDrawByBase(PlayerDraws, Base));
                    }
                }
                #endregion

                #region Entity ESP
                for (int i = 0; i < 1000; i++)
                {
                    int Base = BHelper.EntityList.Base(i);
                    if (Base == 0) continue;

                    int ClassID = BHelper.EntityList.GetEntityID(Base);
                    if (Enum.IsDefined(typeof(BHelper.AcceptedGrenadeIDS), ClassID))
                    {
                        float[] EntityPos = BHelper.EntityList.GetPosition(i);
                        float[] PlayerPos = BHelper.LocalPlayer.GetPosition();
                        float[] ScreenPos = new float[2];

                        if (WorldToScreen(i, EntityPos, ref ScreenPos))
                        {
                            BHelper.EntityDraw EntityInfo = new BHelper.EntityDraw();
                            EntityInfo.EntityBase = Base;
                            EntityInfo.EntityClassID = ClassID;
                            EntityInfo.EntityID = i;

                            EntityInfo.EntityModel = BHelper.EntityList.GetModel(i);

                            EntityInfo.espX = ScreenPos[0];
                            EntityInfo.espY = ScreenPos[1];
                            EntityInfo.espDistance = (float)Math.Sqrt((EntityPos[0] - PlayerPos[0]) * (EntityPos[0] - PlayerPos[0]) + (EntityPos[1] - PlayerPos[1]) * (EntityPos[1] - PlayerPos[1]) + (EntityPos[2] - PlayerPos[2]) * (EntityPos[2] - PlayerPos[2]));

                            int EntityIndex = FindEntityDrawIndexByBase(EntityDraws, Base);
                            if (EntityIndex == -1)
                                EntityDraws.Add(EntityInfo);
                            else
                                EntityDraws[EntityIndex] = EntityInfo;
                        }
                    }
                }

                for (int i = 0; i < EntityDraws.Count; i++)
                {
                    if (EntityDraws[i].EntityBase != BHelper.EntityList.Base(EntityDraws[i].EntityID))
                    {
                        EntityDraws.Remove(FindEntityDrawByBase(EntityDraws, EntityDraws[i].EntityBase));
                    }
                }
                #endregion
                Thread.Sleep(1);
            }
        }

        #region Methods
        static BHelper.PlayerDraw FindPlayerDrawByBase(List<BHelper.PlayerDraw> list, int Base)
        {
            BHelper.PlayerDraw toRemove = new BHelper.PlayerDraw();
            foreach(BHelper.PlayerDraw PlayerInfo in list)
            {
                if (PlayerInfo.PlayerBase == Base)
                {
                    toRemove = PlayerInfo;
                    break;
                }
            }
            return toRemove;
        }

        static int FindPlayerDrawIndexByBase(List<BHelper.PlayerDraw> list, int Base)
        {
            int index = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].PlayerBase == Base)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        static BHelper.EntityDraw FindEntityDrawByBase(List<BHelper.EntityDraw> list, int Base)
        {
            BHelper.EntityDraw toRemove = new BHelper.EntityDraw();
            foreach (BHelper.EntityDraw EntityInfo in list)
            {
                if (EntityInfo.EntityBase == Base)
                {
                    toRemove = EntityInfo;
                    break;
                }
            }
            return toRemove;
        }

        static int FindEntityDrawIndexByBase(List<BHelper.EntityDraw> list, int Base)
        {
            int index = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].EntityBase == Base)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        static internal bool WorldToScreen(int PlayerNumber, float[] from, ref float[] to)
        {
            from = VelocityComp(PlayerNumber, from);

            float[] Matrix = BHelper.WorldToScreen();
            float w = 0.0f;

            to[0] = Matrix[0] * from[0] + Matrix[1] * from[1] + Matrix[2] * from[2] + Matrix[3];
            to[1] = Matrix[4] * from[0] + Matrix[5] * from[1] + Matrix[6] * from[2] + Matrix[7];
            w = Matrix[12] * from[0] + Matrix[13] * from[1] + Matrix[14] * from[2] + Matrix[15];

            if (w < 0.01f)
                return false;

            float invw = 1.0f / w;
            to[0] *= invw;
            to[1] *= invw;

            int width = (int)(1280 - 0);
            int height = (int)(1024 - 0);

            float x = width / 2;
            float y = height / 2;

            x += 0.5f * to[0] * width + 0.5f;
            y -= 0.5f * to[1] * height + 0.5f;

            to[0] = x + 0;
            to[1] = y + 0;

            return true;
        }

        static internal float[] VelocityComp(int PlayerNumber, float[] EnemyPos)
        {
            float[] EnemyVelocity = new float[3];
            float[] MyVelocity = new float[3];
            EnemyVelocity = BHelper.EntityList.GetVelocity(PlayerNumber);
            MyVelocity = BHelper.LocalPlayer.GetVelocity();

            EnemyPos[0] = EnemyPos[0] + (EnemyVelocity[0] / 100) * (40 / 12);
            EnemyPos[1] = EnemyPos[1] + (EnemyVelocity[1] / 100) * (40 / 12);
            EnemyPos[2] = EnemyPos[2] + (EnemyVelocity[2] / 100) * (40 / 12);
            EnemyPos[0] = EnemyPos[0] - (MyVelocity[0] / 100) * (40 / 12);
            EnemyPos[1] = EnemyPos[1] - (MyVelocity[1] / 100) * (40 / 12);
            EnemyPos[2] = EnemyPos[2] - (MyVelocity[2] / 100) * (40 / 12);

            return EnemyPos;
        }
        #endregion

        static void Main()
        {
            while (!BHelper.BHMem.Attach()) Thread.Sleep(1000);

            BHelper.DwClientBase = BHelper.BHMem.GetModule("client.dll");
            BHelper.DwEngineBase = BHelper.BHMem.GetModule("engine.dll");

            Thread ESP = new Thread(new ThreadStart(ThreadESP));
            ESP.Start();

            DirectXWindow();
        }

        [STAThread]
        static void DirectXWindow()
        {
            Application.EnableVisualStyles();
            Application.Run(new BDirectX());
        }
    }
}
