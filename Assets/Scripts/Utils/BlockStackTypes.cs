using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockStackTypes
{

    [Serializable]
    public class AudioEffect
    {
        public string Name;
        public AudioSource AudioSource;

        public override string ToString()
        {
            return Name;
        }

        public AudioEffect(AudioSource audioSource)
        {
            Name = audioSource.clip.name;
            AudioSource = audioSource;
        }
    }

    public enum Themes
    {
        Blue,
        Grey,
        Cyan,
        Skyblue,
        Red
    }

    [Serializable]
    public class GraphicsTheme
    {
        public Themes Theme;
        public Material BaseMaterial;
        public Material BeltMaterial;
        public Material BeltWallMaterial;
        public Material DividerMaterial;
        public Material TrayDispenserMaterial;
        public Material TableMaterial;
        public Material TableBaseMaterial;
        public Material ShadowMaterial;
        public ColorData TrayDispenserBaseColor;
        public ColorData ArrowColor;

        public override string ToString()
        {
            return Theme.ToString();
        }
    }

    [Serializable]
    public class TrayTrailColors
    {
        public string MaterialName;
        public Color MaterialColor;
    }

    public class TrayMaterialColors
    {
        public string MaterialName;
        public Color MaterialColor;
    }

    [Serializable]
    public class CupStacks
    {
        public Stack<string> Identifiers = new();
        public Stack<Transform> Cups = new();
        public Stack<CupColors> Colors = new();
    }

    #region  GAMEPLAY

    [Serializable]
    public class TrayDispenserData
    {
        public TrayOrientation Orientation;
        public bool Active;
        public Vector2 Position;
        public List<Tray> TraysToSpawn;

        public TrayDispenserData(TrayOrientation Orientation, bool Active, Vector3? Position = null)
        {
            this.Orientation = Orientation;
            this.Active = Active;
            if (Position.HasValue) this.Position = new Vector2(Position.Value.x, Position.Value.z);
            TraysToSpawn = new List<Tray>();
        }
    }

    [Serializable]
    public enum TrayOrientation
    {
        Left,
        Right,
        Up,
        Down
    }

    [Serializable]
    public class Conveyer
    {
        public Transform ConveyerBelt;
        public Transform[] LayoutOptions;

        public Conveyer(Transform Belt)
        {
            ConveyerBelt = Belt;
            LayoutOptions = new Transform[ConveyerBelt.childCount];
            for (int i = 0; i < ConveyerBelt.childCount; i++)
            {
                LayoutOptions[i] = ConveyerBelt.transform.GetChild(i);
            }
        }
    }

    [Serializable]
    public enum CupColors
    {
        Blue,
        Brown,
        Cyan,
        Green,
        Orange,
        Pink,
        Purple,
        Red,
        Yellow,
        Teal,
        BlueDesigner,
        BrownDesigner,
        CyanDesigner,
        GreenDesigner,
        OrangeDesigner,
        PinkDesigner,
        PurpleDesigner,
        RedDesigner,
        YellowDesigner,
        TealDesigner
    }

    [Serializable]
    public enum TrayType
    {
        OneHoledTray,
        TwoHoledTray,
        ThreeHoledTray,
        FourHoledTray,
        FiveHoledTray,
        SixHoledTray,
        SevenHoledTray,
        EightHoledTray,
    }

    [Serializable]
    public enum TrayMaterial
    {
        Blue,
        Brown,
        Cyan,
        Green,
        Orange,
        Pink,
        Purple,
        Red,
        Yellow,
        Teal,
        BlueDesigner,
        BrownDesigner,
        CyanDesigner,
        GreenDesigner,
        OrangeDesigner,
        PinkDesigner,
        PurpleDesigner,
        RedDesigner,
        YellowDesigner,
        TealDesigner
    }

    [Serializable]
    public class Tray
    {
        public TrayType Type;
        public TrayMaterial Material;
        public bool IsMysterious = false;
        public int TrayHolesCount => (int)Type + 1;
        public bool IsStarredTray => Material.ToString().Contains("Designer");
        public Color GetTrayColor()
        {
            var baseMat = Material.ToString().Replace("Designer", "");

            return baseMat switch
            {
                "Blue" => Color.blue,
                "Brown" => new Color(0.59f, 0.29f, 0f),
                "Cyan" => Color.cyan,
                "Green" => Color.green,
                "Orange" => new Color(1f, 0.5f, 0f),
                "Pink" => new Color(1f, 0.41f, 0.71f),
                "Purple" => new Color(0.5f, 0f, 0.5f),
                "Red" => Color.red,
                "Yellow" => Color.yellow,
                "Teal" => new Color(0f, 0.5f, 0.5f),
                _ => Color.white
            };
        }
    }

    [Serializable]
    public class Cup
    {
        public CupColors Color;
        public bool IsMysterious = false;
        public Cup(CupColors color, bool IsMysterious = false)
        {
            Color = color;
            this.IsMysterious = IsMysterious;
        }
    }

    [Serializable]
    public class CupStack
    {
        public List<Cup> Cups = new();
        public int Count => Cups.Count;
        public int LockedStackUntil;

        public CupStack(int cupsCount, int LockedStackUntil = -1)
        {
            this.LockedStackUntil = LockedStackUntil;
            for (int i = 0; i < cupsCount; i++)
            {
                Cups.Add(new Cup(CupColors.Blue));
            }
        }

        public void SetLockedStackUntil(int LockedStackUntil)
        {
            this.LockedStackUntil = LockedStackUntil;
        }

        public void AddCup(CupColors color, bool IsMysterious = false)
        {
            Cups.Add(new Cup(color, IsMysterious));
        }

        public void RemoveCup()
        {
            if (Cups.Count > 0)
            {
                Cups.RemoveAt(Cups.Count - 1);
            }
        }
    }

    [Serializable]
    public class CupStackRow
    {
        public List<CupStack> CupStacks = new();
        public int Count => CupStacks.Count;

        public CupStackRow(List<CupStack> stacks)
        {
            CupStacks = stacks;
        }
    }

    [Serializable]
    public class CupGrid3D
    {
        public int Width;
        public int Height;
        public List<CupStackRow> Grid;

        public CupGrid3D(int width, int height)
        {
            Width = width;
            Height = height;
            Grid = new List<CupStackRow>();

            for (int i = 0; i < Width; i++)
            {
                List<CupStack> stacks = new List<CupStack>();
                for (int j = 0; j < Height; j++)
                {
                    stacks.Add(new CupStack(0));
                }
                Grid.Add(new CupStackRow(stacks));
            }
        }

        public bool IsCupMysterious(int j, int i, int k)
        {
            return Grid[j].CupStacks[i].Cups[k].IsMysterious;
        }

        public (Color, bool) GetCupColor(int x, int y, int z)
        {
            CupColors color = Grid[x].CupStacks[y].Cups[z].Color;
            return color switch
            {
                CupColors.Blue => (new Color(0.0f, 0.0f, 1.0f), false),
                CupColors.BlueDesigner => (new Color(0.1f, 0.5f, 1.0f), true),

                CupColors.Brown => (new Color(0.6f, 0.3f, 0.1f), false),
                CupColors.BrownDesigner => (new Color(0.4f, 0.2f, 0.05f), true),

                CupColors.Cyan => (new Color(0.0f, 1.0f, 1.0f), false),
                CupColors.CyanDesigner => (new Color(0.0f, 0.7f, 1.0f), true),

                CupColors.Green => (new Color(0.0f, 1.0f, 0.0f), false),
                CupColors.GreenDesigner => (new Color(0.0f, 0.6f, 0.3f), true),

                CupColors.Orange => (new Color(1.0f, 0.5f, 0.0f), false),
                CupColors.OrangeDesigner => (new Color(1.0f, 0.3f, 0.0f), true),

                CupColors.Pink => (new Color(1.0f, 0.75f, 0.8f), false),
                CupColors.PinkDesigner => (new Color(0.9f, 0.4f, 0.5f), true),

                CupColors.Purple => (new Color(0.5f, 0.0f, 0.5f), false),
                CupColors.PurpleDesigner => (new Color(0.7f, 0.2f, 0.9f), true),

                CupColors.Red => (new Color(1.0f, 0.0f, 0.0f), false),
                CupColors.RedDesigner => (new Color(0.6f, 0.0f, 0.0f), true),

                CupColors.Yellow => (new Color(1.0f, 1.0f, 0.0f), false),
                CupColors.YellowDesigner => (new Color(1.0f, 0.85f, 0.1f), true),

                CupColors.Teal => (new Color(0.0f, 0.5f, 0.5f), false),
                CupColors.TealDesigner => (new Color(0.0f, 0.3f, 0.7f), true),

                _ => (Color.white, false),
            };
        }


        public void AddCup(int x, int y, CupColors color, bool IsMysterious = false)
        {
            Grid[x].CupStacks[y].AddCup(color, IsMysterious);
        }

        public void RemoveCup(int x, int y)
        {
            Grid[x].CupStacks[y].RemoveCup();
        }

        public int GetStackHeight(int x, int y)
        {
            return Grid[x].CupStacks[y].Count;
        }

        public CupStack GetStack(int i, int j)
        {
            return Grid[i].CupStacks[j];
        }

        public void SetMaterial(int j, int i, CupColors color)
        {
            int LastCupIndex = Grid[j].CupStacks[i].Count - 1;
            Grid[j].CupStacks[i].Cups[LastCupIndex].Color = color;
        }

        public void SetCupColor(int j, int i, CupColors cupColor)
        {
            int LastCupIndex = Grid[j].CupStacks[i].Count - 1;
            Grid[j].CupStacks[i].Cups[LastCupIndex].Color = cupColor;
        }
    }
    #endregion

    #region HAPTICS
    public enum HapticType
    {
        None,
        Light,
        Medium,
        Heavy,
        Warning,
        Failure,
        Success,
        Default,
        Vibrate,
        Selection
    }
    #endregion
}

