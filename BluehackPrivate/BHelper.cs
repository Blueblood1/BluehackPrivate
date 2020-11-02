namespace SubService
{
    static class BHelper
    {
        #region Initalisers
        static internal BMemory BHMem = new BMemory();
        static internal int DwClientBase;
        static internal int DwEngineBase;

        static private int DwLocalPlayer = 0xAADFFC;
        static private int DwEntityList = 0x4A8A684;
        static private int DwViewMatrix = 0x4A7C0E4;

        static private int DwHealth = 0x000000FC;
        static private int DwLifeState = 0x0000025B;
        static private int DwTeam = 0x000000F0;
        static private int DwPosition = 0x00000134;
        static private int DwBoneMatrix = 0x00002698;
        static private int DwDormant = 0x000000E9;
        static private int DwVelocity = 0x110;
        static private int DwPunch = 0x301C;
        #endregion

        internal struct LocalPlayer
        {
            static internal int Base()
            {
                return BHMem.ReadMemory<int>(DwClientBase + DwLocalPlayer);
            }

            static internal int GetTeam()
            {
                return BHMem.ReadMemory<int>(Base() + DwTeam);
            }

            static internal bool GetLifeState()
            {
                return BHMem.ReadMemory<bool>(Base() + DwLifeState);
            }

            static internal float[] GetPosition()
            {
                return BHMem.ReadMatrix<float>(Base() + DwPosition, 3);
            }

            static internal float[] GetVelocity()
            {
                return BHMem.ReadMatrix<float>(Base() + DwVelocity, 3);
            }

            static internal float[] GetPunch()
            {
                return BHMem.ReadMatrix<float>(Base() + DwPunch, 3);
            }
        }

        internal struct EntityList
        {
            static internal int Base(int PlayerID)
            {
                return BHMem.ReadMemory<int>(DwClientBase + DwEntityList + (0x10 * PlayerID));
            }

            static internal int GetTeam(int PlayerID)
            {
                return BHMem.ReadMemory<int>(Base(PlayerID) + DwTeam);
            }

            static internal int GetHealth(int PlayerID)
            {
                return BHMem.ReadMemory<int>(Base(PlayerID) + DwHealth);
            }

            static internal int GetEntityID(int EntityBase)
            {
                int vt = BHMem.ReadMemory<int>(EntityBase + 0x8);
                int fn = BHMem.ReadMemory<int>(vt + 2 * 0x4);
                int cls = BHMem.ReadMemory<int>(fn + 0x1);
                return BHMem.ReadMemory<int>(cls + 0x14);
            }

            static internal bool GetLifeState(int PlayerID)
            {
                return !BHMem.ReadMemory<bool>(Base(PlayerID) + DwLifeState);
            }

            static internal bool GetDormant(int PlayerID)
            {
                return BHMem.ReadMemory<bool>(Base(PlayerID) + DwDormant);
            }

            static internal float[] GetBonePosition(int PlayerID, int BoneID)
            {
                int BoneMatrix = BHMem.ReadMemory<int>(Base(PlayerID) + DwBoneMatrix);

                float[] Pos = new float[3];
                Pos[0] = BHMem.ReadMemory<float>(BoneMatrix + 0x30 * BoneID + 0x0C);
                Pos[1] = BHMem.ReadMemory<float>(BoneMatrix + 0x30 * BoneID + 0x1C);
                Pos[2] = BHMem.ReadMemory<float>(BoneMatrix + 0x30 * BoneID + 0x2C);
                return Pos;
            }

            static internal float[] GetPosition(int PlayerID)
            {
                return BHMem.ReadMatrix<float>(Base(PlayerID) + DwPosition, 3);
            }

            static internal float[] GetVelocity(int PlayerID)
            {
                return BHMem.ReadMatrix<float>(Base(PlayerID) + DwVelocity, 3);
            }
            
            static internal string GetModel(int PlayerID)
            {
                int ModelBase = BHMem.ReadMemory<int>(Base(PlayerID) + 0x6C);
                return System.Text.Encoding.ASCII.GetString(BHMem.ReadBytes(ModelBase + 0x0004, 260), 0, 32);
            }
        }

        internal struct PlayerDraw
        {
            internal int PlayerBase;
            internal int PlayerID;
            internal int PlayerTeam;
            internal int PlayerHealth;

            internal float espX;
            internal float espY;
            internal float espDistance;

            internal float[] Bones;
        }

        internal struct EntityDraw
        {
            internal int EntityBase;
            internal int EntityID;
            internal int EntityClassID;

            internal string EntityModel;

            internal float espX;
            internal float espY;
            internal float espDistance;
        }

        internal static float[] WorldToScreen()
        {
            return BHMem.ReadMatrix<float>(DwClientBase + DwViewMatrix, 16);
        }

        internal enum AcceptedGrenadeIDS
        {
            CSmokeGrenadeProjectile = 134,
            CDecoyProjectile = 41,
            CMolotovProjectile = 98,
            CBaseCSGrenadeProjectile = 9
        }
    }
}
