using UnityEditor;
using UnityEngine;

public class PT_PackageWelcomeWindow : EditorWindow
{
    private const string PrefKey = "PT_WelcomeScreen";

    private static Texture2D banner;
    private static Texture2D iconTwitter;
    private static Texture2D iconYouTube;
    private static Texture2D iconFacebook;
    private static Texture2D iconInstagram;
    private static Texture2D iconTikTok;
    private static Texture2D iconArtStation;

    private static Texture2D demoIcon1;
    private static Texture2D demoIcon2;

    private static Texture2D gameIcon1;
    private static Texture2D gameIcon2;
    private static Texture2D gameIcon3;
    private static Texture2D gameIcon4;
    private static Texture2D gameIcon5;
    private static Texture2D gameIcon6;

    private bool dontShowAgain = true;
    private bool demosExpanded;
    private bool gamesExpanded;

    private const float WindowWidth       = 500f;
    private const float BannerHeight      = 160f;
    private const float SocialIconSize    = 24f;
    private const float SocialIconSpacing = 6f;
    private const float CardSpacing       = 8f * 1.21f;
    private const float CardScale         = 0.7f;
    private const float CaptionHeight     = 65f;
    private const float LinkButtonWidth   = (WindowWidth - 56f) / 4f * 1.05f;

    // ─── Detect package import (custom package or Package Manager) ────────────
    private class PT_ImportDetector : AssetPostprocessor
    {
        // Marker asset that is part of this package. Adjust the path if needed.
        private const string MarkerAsset =
            "Assets/Polytope Studio/Welcome_Screen/Editor/Textures/banner_dark.png";

        // SessionState key so we only trigger once per Unity session even if
        // several assets arrive in the same import batch.
        private const string SessionKey = "PT_WelcomeShownThisSession";

        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (SessionState.GetBool(SessionKey, false))
                return;

            foreach (string path in importedAssets)
            {
                if (path == MarkerAsset)
                {
                    SessionState.SetBool(SessionKey, true);
                    // Reset the "don't show" pref so the window appears after import.
                    EditorPrefs.SetBool(PrefKey, false);
                    LoadAssets();
                    EditorApplication.update += ShowOnLoad;
                    break;
                }
            }
        }
    }
    // ─────────────────────────────────────────────────────────────────────────

    [InitializeOnLoadMethod]
    private static void OnProjectLoaded()
    {
        if (!EditorPrefs.GetBool(PrefKey, false))
        {
            LoadAssets();
            EditorApplication.update += ShowOnLoad;
        }
    }

    [MenuItem("Tools/Polytope/Welcome Screen")]
    public static void OpenWindow()
    {
        LoadAssets();
        var window = GetWindow<PT_PackageWelcomeWindow>(true, "Welcome");
        window.minSize = new Vector2(WindowWidth, 0);
        window.maxSize = new Vector2(WindowWidth, 5000);
        window.Show();
    }

    private static void ShowOnLoad()
    {
        EditorApplication.update -= ShowOnLoad;
        OpenWindow();
    }

    private void OnEnable()
    {
        // Initialise the toggle to whatever the user last saved.
        // GetBool returns false when the pref is absent (first ever run),
        // so dontShowAgain will be false until the user checks the box and closes.
        dontShowAgain = !EditorPrefs.GetBool(PrefKey, false);
        minSize = new Vector2(WindowWidth, 0);
        maxSize = new Vector2(WindowWidth, 5000);
    }

    private static void LoadAssets()
    {
        string bannerPath = EditorGUIUtility.isProSkin
            ? "Assets/Polytope Studio/Welcome_Screen/Editor/Textures/banner_dark.png"
            : "Assets/Polytope Studio/Welcome_Screen/Editor/Textures/banner_light.png";

        banner         = AssetDatabase.LoadAssetAtPath<Texture2D>(bannerPath);
        iconTwitter    = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Welcome_Screen/Editor/Textures/icon_twitter.png");
        iconYouTube    = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Welcome_Screen/Editor/Textures/icon_youtube.png");
        iconFacebook   = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Welcome_Screen/Editor/Textures/icon_facebook.png");
        iconInstagram  = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Welcome_Screen/Editor/Textures/icon_instagram.png");
        iconTikTok     = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Welcome_Screen/Editor/Textures/icon_tiktok.png");
        iconArtStation = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Welcome_Screen/Editor/Textures/icon_artstation.png");

        demoIcon1 = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Welcome_Screen/Editor/Textures/demo1.png");
        demoIcon2 = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Welcome_Screen/Editor/Textures/demo2.png");

        gameIcon1 = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Welcome_Screen/Editor/Textures/game1.png");
        gameIcon2 = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Welcome_Screen/Editor/Textures/game2.png");
        gameIcon3 = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Welcome_Screen/Editor/Textures/game3.png");
        gameIcon4 = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Welcome_Screen/Editor/Textures/game4.png");
        gameIcon5 = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Welcome_Screen/Editor/Textures/game5.png");
        gameIcon6 = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Welcome_Screen/Editor/Textures/game6.png");
    }

    private static GUIStyle SectionTitleStyle() =>
        new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };

    private static GUIStyle SectionDescStyle() =>
        new GUIStyle(EditorStyles.wordWrappedMiniLabel)
        {
            normal   = { textColor = EditorStyles.wordWrappedMiniLabel.normal.textColor },
            wordWrap = true,
            fontSize = 11
        };

    private void DrawSectionDesc(string text)
    {
        GUIStyle style  = SectionDescStyle();
        float    width  = WindowWidth - 24f;
        float    height = Mathf.Max(style.CalcHeight(new GUIContent(text), width), 36f);
        GUILayout.Label(text, style, GUILayout.Height(height));
    }

    private void OnGUI()
    {
        // Banner
        if (banner)
        {
            Rect bannerRect = GUILayoutUtility.GetRect(WindowWidth, BannerHeight);
            GUI.DrawTexture(bannerRect, banner, ScaleMode.StretchToFill);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize  = 18,
                normal    = { textColor = Color.white },
                hover     = { textColor = Color.white }
            };
            GUIStyle descStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = Color.white },
                hover     = { textColor = Color.white }
            };

            float titleH = 28f;
            float descH  = 20f;
            float startY = bannerRect.yMax - titleH - descH - 5f;

            GUI.Label(new Rect(bannerRect.x, startY,          bannerRect.width, titleH), "Thank you for trusting our assets!", titleStyle);
            GUI.Label(new Rect(bannerRect.x, startY + titleH, bannerRect.width, descH),  "Below are some useful resources to help you get started.", descStyle);
        }

        GUILayout.Space(10);

        // Discord
        DrawPanel(() =>
        {
            GUILayout.Label("💬  Discord Community", SectionTitleStyle());
            GUILayout.Label("Join our community, share your project, ask for support, and win free vouchers every month.", EditorStyles.wordWrappedMiniLabel);
            GUILayout.Space(6);
            DrawDiscordGallery();
        });

        GUILayout.Space(8);

        // Games / Demos
        DrawPanel(() =>
        {
            GUILayout.Space(4);
            DrawGamesGallery();
        });

        GUILayout.Space(8);

        // Docs / Support
        DrawPanel(() =>
        {
            GUILayout.Label("📋  Docs, Support, Reviews & Roadmap", SectionTitleStyle());
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical(GUILayout.Width(LinkButtonWidth));
            DrawLink("Documentation", "Documentations for every pack.",
                "https://drive.google.com/drive/folders/1mcjjdqr91Qt38Jhko_64K0E1Kx-Efh31?usp=drive_link");
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(LinkButtonWidth));
            DrawLink("Support Email", "Send us a question or report an issue.",
                "mailto:contact@polytope.studio");
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(LinkButtonWidth));
            DrawLink("⭐ Rate & Review", "Enjoyed our assets? Leave us a review!",
                "https://assetstore.unity.com/publishers/35251");
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(LinkButtonWidth));
            DrawLink("🗺 Roadmap", "See what we're building next.",
                "https://trello.com/b/HTwC0zvm/polytope-studio-lowpoly-assets");
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        });

        GUILayout.Space(8);

        // Social
        DrawPanel(() =>
        {
            GUILayout.Label("🌐  Follow Us", SectionTitleStyle());
            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawSocial(iconTwitter,    "https://x.com/PolytopeStudio");
            DrawSocial(iconYouTube,    "https://www.youtube.com/@polytopestudio");
            DrawSocial(iconFacebook,   "https://www.facebook.com/PolytopeStudio");
            DrawSocial(iconInstagram,  "https://www.instagram.com/polytopestudio/");
            DrawSocial(iconTikTok,     "https://www.tiktok.com/@polytopestudio");
            DrawSocial(iconArtStation, "https://www.artstation.com/polytope/store");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        });

        GUILayout.Space(8);

        dontShowAgain = EditorGUILayout.ToggleLeft("Don't show this again", dontShowAgain);
        GUILayout.Space(8);

        if (GUILayout.Button("Close", GUILayout.Height(30)))
        {
            // Always persist the user's choice explicitly in both directions.
            EditorPrefs.SetBool(PrefKey, dontShowAgain);
            Close();
        }

        if (Event.current.type == EventType.Repaint)
        {
            float h = GUILayoutUtility.GetLastRect().yMax + 20f;
            minSize = new Vector2(WindowWidth, h);
            maxSize = new Vector2(WindowWidth, h);
        }
    }

    private void DrawDiscordGallery()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUILayout.BeginVertical(GUILayout.Width(LinkButtonWidth));
        DrawLink("Join Server", "Meet the community.", "https://discord.com/invite/SZ6whXU");
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.Width(LinkButtonWidth));
        DrawLink("#giveaway", "Free vouchers monthly.", "https://discord.gg/DAKGgUu2yE");
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.Width(LinkButtonWidth));
        DrawLink("#unity-support", "Get help with our assets.", "https://discord.gg/YAKSckftfn");
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.Width(LinkButtonWidth));
        DrawLink("#FAQ", "Frequently asked questions.", "https://discord.gg/PPbYzb5tRT");
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void DrawGamesGallery()
    {
        float available      = WindowWidth - 12f;
        float cardWidth      = (available - CardSpacing * 2f) / 3f * CardScale * 1.2f;
        float cardHeight     = cardWidth * (9f / 16f);
        float demoCardWidth  = available * 0.4f * CardScale;
        float demoCardHeight = demoCardWidth * (9f / 16f);
        float rowH           = cardHeight     + CaptionHeight * 1.1f;
        float demoRowH       = demoCardHeight + CaptionHeight;

        DrawCollapsibleSeparator("Polytope Demos", ref demosExpanded);
        if (demosExpanded)
        {
            DrawSectionDesc("Try our interactive demos and see our assets in action.");
            GUILayout.Space(6f);
            DrawCardRow(demoRowH, () =>
            {
                GUILayout.FlexibleSpace();
                DrawCardCentered(demoIcon1, "https://apps.microsoft.com/detail/9n4hbjpcznqn?hl=es-ES&gl=ES", demoCardWidth, demoCardHeight, "Mix and preview modular armor sets with real-time character customization");
                HGap();
                DrawCardCentered(demoIcon2, "https://apps.microsoft.com/detail/9pl3wkh940q7?hl=es-ES&gl=ES", demoCardWidth, demoCardHeight, "Explore a stylized village built entirely with the our modular assets.");
                GUILayout.FlexibleSpace();
            });
            GUILayout.Space(4f);
        }

        GUILayout.Space(6f);

        DrawCollapsibleSeparator("Made with our Assets", ref gamesExpanded);
        if (gamesExpanded)
        {
            DrawSectionDesc("Games built by the community using our asset packs.");
            GUILayout.Space(6f);
            DrawCardRow(rowH, () =>
            {
                GUILayout.FlexibleSpace();
                DrawCardCentered(gameIcon1, "https://store.steampowered.com/app/3722910/Legends_of_Azamar_Demo/",      cardWidth, cardHeight, "Step into the world of Avendor and brave the ruins of Bar-Ulduun, the fabled lost city of the dwarves",         CaptionHeight * 1.1f);
                HGap();
                DrawCardCentered(gameIcon2, "https://store.steampowered.com/app/2932960/A_Merchants_Promise/",         cardWidth, cardHeight, "Medieval Trading & Transport with Physics-Based Items",                                                            CaptionHeight * 1.1f);
                HGap();
                DrawCardCentered(gameIcon3, "https://store.steampowered.com/app/1434840/Dungeons__Kingdoms_Prologue/", cardWidth, cardHeight, "A medieval fantasy kingdom builder, management sim and dungeon delver RPG hybrid",                                  CaptionHeight * 1.1f);
                GUILayout.FlexibleSpace();
            });
            GUILayout.Space(5f);
            DrawCardRow(rowH, () =>
            {
                GUILayout.FlexibleSpace();
                DrawCardCentered(gameIcon4, "https://store.steampowered.com/app/2369850/Dolven/",                    cardWidth, cardHeight, "A narrative-driven tactical RPG where combat blends party-based skills with poker-style card combos", CaptionHeight * 1.1f);
                HGap();
                DrawCardCentered(gameIcon5, "https://store.steampowered.com/app/1228500/1428_Shadows_over_Silesia/", cardWidth, cardHeight, "Immerse yourself in a dark fantasy story with true historical events",                              CaptionHeight * 1.1f);
                HGap();
                DrawCardCentered(gameIcon6, "https://store.steampowered.com/app/3516100/Tenebyss/",                  cardWidth, cardHeight, "A brutal souls-like extraction adventure game set in massive dystopian worlds",                      CaptionHeight * 1.1f);
                GUILayout.FlexibleSpace();
            });
            GUILayout.Space(4f);
        }
    }

    private void DrawCollapsibleSeparator(string label, ref bool expanded)
    {
        GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter
        };
        GUIStyle arrowStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleRight,
            normal    = { textColor = new Color(0.6f, 0.6f, 0.6f, 1f) },
            hover     = { textColor = new Color(0.9f, 0.9f, 0.9f, 1f) },
            fontSize  = 10
        };

        Vector2 labelSize   = labelStyle.CalcSize(new GUIContent(label));
        float   rowHeight   = Mathf.Max(labelSize.y, 18f);
        Rect    rowRect     = GUILayoutUtility.GetRect(0f, rowHeight, GUILayout.ExpandWidth(true), GUILayout.Height(rowHeight));

        if (rowRect.Contains(Event.current.mousePosition))
        {
            EditorGUI.DrawRect(rowRect, new Color(1f, 1f, 1f, 0.04f));
            EditorGUIUtility.AddCursorRect(rowRect, MouseCursor.Link);
        }

        float arrowW        = 20f;
        float labelW        = labelSize.x + 12f;
        float halfRemainder = (rowRect.width - labelW - arrowW) * 0.5f;
        float lineY         = rowRect.y + rowRect.height * 0.5f;
        float lineMargin    = 4f;

        EditorGUI.DrawRect(new Rect(rowRect.x, lineY, halfRemainder - lineMargin, 1f), new Color(1f, 1f, 1f, 0.15f));
        GUI.Label(new Rect(rowRect.x + halfRemainder, rowRect.y, labelW, rowHeight), label, labelStyle);
        EditorGUI.DrawRect(new Rect(rowRect.x + halfRemainder + labelW + lineMargin, lineY, halfRemainder - lineMargin, 1f), new Color(1f, 1f, 1f, 0.15f));
        GUI.Label(new Rect(rowRect.xMax - arrowW, rowRect.y, arrowW, rowHeight), expanded ? "▼" : "▶", arrowStyle);

        if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition))
        {
            expanded = !expanded;
            Event.current.Use();
            Repaint();
        }
    }

    private void DrawCardRow(float rowHeight, System.Action content)
    {
        GUILayout.BeginHorizontal(GUILayout.Height(rowHeight));
        content();
        GUILayout.EndHorizontal();
    }

    private void HGap()
    {
        GUILayout.Label(GUIContent.none, GUIStyle.none, GUILayout.Width(CardSpacing), GUILayout.Height(0));
    }

    private void DrawCardCentered(Texture2D icon, string url, float cardWidth, float cardHeight, string caption = null, float captionHeight = CaptionHeight)
    {
        GUIStyle captionStyle = new GUIStyle(EditorStyles.wordWrappedMiniLabel)
        {
            alignment = TextAnchor.UpperCenter,
            normal    = { textColor = EditorStyles.wordWrappedMiniLabel.normal.textColor },
            wordWrap  = true,
            fontSize  = 11
        };

        float totalH = cardHeight + (caption != null ? 3f + captionHeight : 0f);
        GUILayout.BeginVertical(GUILayout.Width(cardWidth), GUILayout.Height(totalH));
        GUILayout.FlexibleSpace();
        DrawIconCard(icon, url, cardWidth, cardHeight);
        if (caption != null)
        {
            GUILayout.Space(3f);
            GUILayout.Label(caption, captionStyle, GUILayout.Width(cardWidth), GUILayout.Height(captionHeight));
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
    }

    private void DrawIconCard(Texture2D icon, string url, float cardWidth, float cardHeight)
    {
        Rect cardRect = GUILayoutUtility.GetRect(cardWidth, cardHeight, GUILayout.Width(cardWidth), GUILayout.Height(cardHeight));

        if (icon)
            GUI.DrawTexture(cardRect, icon, ScaleMode.ScaleToFit);

        float bw = 2f;
        EditorGUI.DrawRect(new Rect(cardRect.x,         cardRect.y,         cardRect.width, bw),              new Color(0.8f, 0.8f, 0.8f, 0.8f));
        EditorGUI.DrawRect(new Rect(cardRect.x,         cardRect.yMax - bw, cardRect.width, bw),              new Color(0.8f, 0.8f, 0.8f, 0.8f));
        EditorGUI.DrawRect(new Rect(cardRect.x,         cardRect.y,         bw,             cardRect.height), new Color(0.8f, 0.8f, 0.8f, 0.8f));
        EditorGUI.DrawRect(new Rect(cardRect.xMax - bw, cardRect.y,         bw,             cardRect.height), new Color(0.8f, 0.8f, 0.8f, 0.8f));

        if (cardRect.Contains(Event.current.mousePosition))
        {
            EditorGUI.DrawRect(cardRect, new Color(1f, 1f, 1f, 0.08f));
            EditorGUIUtility.AddCursorRect(cardRect, MouseCursor.Link);
        }

        if (GUI.Button(cardRect, GUIContent.none, GUIStyle.none))
            Application.OpenURL(url);
    }

    private void DrawPanel(System.Action content)
    {
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Space(4);
        content();
        GUILayout.Space(4);
        GUILayout.EndVertical();
    }

    private void DrawLink(string title, string description, string url)
    {
        if (GUILayout.Button(title, GUILayout.Height(26)))
            Application.OpenURL(url);
        GUILayout.Label(description, EditorStyles.wordWrappedMiniLabel);
        GUILayout.Space(6);
    }

    private void DrawSocial(Texture2D icon, string url)
    {
        if (!icon) return;

        GUILayout.BeginVertical(GUILayout.Width(SocialIconSize), GUILayout.Height(SocialIconSize));
        GUILayout.FlexibleSpace();

        Rect r = GUILayoutUtility.GetRect(SocialIconSize, SocialIconSize, GUILayout.Width(SocialIconSize), GUILayout.Height(SocialIconSize));
        if (GUI.Button(r, icon, GUIStyle.none))
            Application.OpenURL(url);

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();

        GUILayout.Space(SocialIconSpacing);
    }
}
