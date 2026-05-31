namespace Core.UI
{
    /// <summary>
    /// ElaAwesome 字体图标码点映射。
    /// 用法：XAML 中 FontFamily="{StaticResource ElaAwesome}" Content="{x:Static cui:ElaIcon.Play}"
    /// </summary>
    public static class ElaIcon
    {
        // ── 常用操作 ──
        public const string Plus = "\uf116";
        public const string Minus = "\uefcb";
        public const string Xmark = "\uf4ce";
        public const string Check = "\uea6c";
        public const string Search = "\uef68";           // MagnifyingGlass
        public const string Play = "\uf10d";
        public const string Stop = "\uf2ed";
        public const string Pause = "\uf063";
        public const string Undo = "\uf189";             // RotateLeft
        public const string Redo = "\uf18d";             // RotateRight
        public const string Save = "\ued59";             // FloppyDisk

        // ── 方向 ──
        public const string ChevronRight = "\uea85";
        public const string ChevronLeft = "\uea84";
        public const string ChevronDown = "\uea83";
        public const string ChevronUp = "\uea8a";
        public const string ArrowRight = "\ue865";
        public const string ArrowLeft = "\ue85b";
        public const string ArrowRotateRight = "\ue871";

        // ── 编辑 ──
        public const string Pen = "\uf06f";
        public const string PenToSquare = "\uf080";
        public const string Copy = "\ueba9";
        public const string Paste = "\uf062";
        public const string Trash = "\uf3a1";
        public const string Scissors = "\uf1bc";

        // ── 节点图标 ──
        public const string Clock = "\ueb18";
        public const string Camera = "\ue9fe";
        public const string Eye = "\uec66";
        public const string Image = "\ueeb5";
        public const string EyeSlash = "\uec6d";
        public const string CameraRetro = "\ue9f7";
        public const string Gear = "\ueda9";
        public const string GearComplex = "\uedab";
        public const string CirclePlay = "\ueae4";
        public const string CircleStop = "\ueaf6";
        public const string CircleCheck = "\ueab4";
        public const string CircleXmark = "\ueb05";
        public const string CircleExclamation = "\ueac7";
        public const string CircleInfo = "\uead0";
        public const string Circle = "\uea94";
        public const string SquareCheck = "\uf27d";
        public const string SquarePlus = "\uf2ab";
        public const string CircleNodes = "\ueadb";
        public const string CircleDollar = "\ueabc";

        // ── 流程控制 ──
        public const string CodeBranch = "\ueb5d";
        public const string CodeMerge = "\ueb63";
        public const string CodeFork = "\ueb60";
        public const string Shuffle = "\uf201";
        public const string Repeat = "\uf165";
        public const string Link = "\uef34";
        public const string Unlock = "\uf400";
        public const string Lock = "\uef59";
        public const string Flag = "\ued4c";
        public const string Hashtag = "\uee30";
        public const string Star = "\uf2d5";
        public const string Heart = "\uee49";
        public const string Bell = "\ue909";
        public const string Signal = "\uf209";
        public const string Filter = "\ued37";

        // ── 布局 / 视图 ──
        public const string Grid = "\uedce";
        public const string Grid2 = "\uedcf";
        public const string LayerGroup = "\uef0e";
        public const string Pallet = "\uf04e";
        public const string Palette = "\uf04f";
        public const string Sliders = "\uf230";
        public const string Expand = "\uec63";
        public const string Maximize = "\uef8e";
        public const string Minimize = "\uefca";
        public const string Ellipsis = "\uec4d";
        public const string EllipsisVertical = "\uec50";
        public const string GripVertical = "\uede1";

        // ── 用户 / 消息 ──
        public const string User = "\uf411";
        public const string Users = "\uf442";
        public const string Comment = "\ueb75";
        public const string Envelope = "\uec54";
        public const string Tags = "\uf32a";
        public const string Bookmark = "\ue947";
        public const string MapPin = "\uef7c";

        // ── 视觉算法（ElaAwesome 等效图标，每个节点唯一）──
        public const string ViGaussianBlur = "\uec2d";       // Droplet
        public const string ViMedianBlur = "\uec5d";         // Eraser
        public const string ViCannyEdge = "\uec28";          // DrawPolygon
        public const string ViThreshold = "\ued9f";          // GaugeHigh
        public const string ViAdaptiveThreshold = "\uf230";   // Sliders
        public const string ViBrightnessContrast = "\uf2ff";  // SunBright
        public const string ViCvtColor = "\uf30c";            // Swatchbook
        public const string ViEqualizeHist = "\uea67";        // ChartSimple
        public const string ViFlip = "\uf161";                // ReflectHorizontal
        public const string ViMorphology = "\uf1d8";          // Shapes
        public const string ViHoughLines = "\uea56";          // ChartLine
        public const string ViResize = "\uec63";              // Expand
        public const string ViImageSource = "\ued0c";         // FileImage
        public const string ViImageDisplay = "\uec02";        // Display
        public const string ViWandMagicSparkles = "\uf48e";   // WandMagicSparkles
        public const string ViSharpen = "\uf48e";             // WandMagicSparkles
        public const string ViSobelEdge = "\ue959";           // BorderAll
        public const string ViBilateralFilter = "\uecc0";     // FaceSmileBeam
        public const string ViRotate = "\uf189";              // Rotate
        public const string ViInRange = "\uec66";             // Eye
        public const string ViBlend = "\uef0e";               // LayerGroup
        public const string ViHoughCircles = "\uea94";       // Circle
        public const string ViCLAHE = "\uea67";              // ChartSimple
        public const string ViGammaCorrection = "\ue9a0";    // Brightness
        public const string ViDistanceTransform = "\ue959";   // BorderAll
        public const string ViGaussianNoise = "\uf234";       // Smog
        public const string ViConnectedComponents = "\uf1d8"; // Shapes
        public const string ViTemplateMatch = "\uf002";      // Search via Music
        public const string ViPerspectiveWarp = "\uef79";     // Map
        public const string ViWatershed = "\uf498";           // Water
        public const string ViLaplacianEdge = "\uec28";       // DrawPolygon
        public const string ViRectROI = "\uf065";             // PawClaws (crop)
        public const string ViMorphGradient = "\uebc3";       // Cube
    }
}
