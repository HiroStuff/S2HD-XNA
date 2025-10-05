using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using S2HD.Animation;
using S2HD.Graphics;
using S2HD.GameStates;
using S2HD.Audio;
using System;
using System.Linq;
using System.Collections.Generic;

namespace S2HD.Title
{
    internal class UserInterface
    {
        private GraphicsDevice _graphicsDevice;
        private ContentManager _content;
        private CustomFont _font;
        private CustomFont _fontImpactRegular;
        private CustomFont _fontImpactItalic;

        private AnimationInstance _miniSonicAniInstance;
        private AnimationInstance _miniTailsAniInstance;

        private Texture2D _selectionMarkerTexture;
        private Texture2D _leftArrowTexture;
        private Texture2D _rightArrowTexture;
        private Texture2D _whiteTexture;
        private Texture2D _zigzagTexture;

        private AudioManager _audioManager;
        private TitleGameState _titleGameState;

        private int _ticks;
        private bool _pressStartActive;
        private Vector2 _pressStartScale;
        private double _pressStartOpacity;
        private double _pressStartWhiteAdditive;
        private double _textWhiteAdditive;
        private int _selectionIndex;
        private int _levelSelectInputState;
        private bool _levelSelectEnabled;
        private int _levelSelectSelectionIndex;
        private int? _demoTimeout;
        private int? _characterSelectTimer;
        private bool _characterSelectActive;
        private double _characterSelectOpacity;
        private double _textOpacity;
        private int _characterSelectionIndex;
        private bool _characterSelected;
        private bool _busy;
        
        private KeyboardState _previousKeyboardState;
        private GamePadState _previousGamePadState;
        
        private EffectEventManager _effectEventManager;

        private string[] _menuItems = { "NEW GAME", "OPTIONS", "QUIT" };
        private string[] _levelSelectItems = { "EMERALD HILL ZONE", "CHEMICAL PLANT ZONE", "AQUATIC RUIN ZONE", "CASINO NIGHT ZONE", "HILL TOP ZONE", "MYSTIC CAVE ZONE", "OIL OCEAN ZONE", "METROPOLIS ZONE", "WING FORTRESS ZONE", "DEATH EGG ZONE" };
        
        private MenuItemWidget[] _menuItemWidgets;
        private Vector2[] _selectedMenuItemMarkerPositions = new Vector2[2];

        public bool Visible { get; set; }

        private bool IsSonicActive => _characterSelectionIndex == 0 || _characterSelectionIndex == 1;
        private bool IsTailsActive => _characterSelectionIndex == 0 || _characterSelectionIndex == 2;

        public UserInterface(GraphicsDevice graphicsDevice, ContentManager content, TitleGameState titleGameState, AudioManager audioManager)
        {
            _graphicsDevice = graphicsDevice;
            _content = content;
            _audioManager = audioManager;
            _titleGameState = titleGameState;
            _effectEventManager = new EffectEventManager();
            LoadContent();
            InitializeLevelSelect();
        }

        private void LoadContent()
        {
            _font = new CustomFont();
            _font.LoadFromXml(_content, "Content/SONICORCA/FONTS/HUD_FONT");

            _fontImpactRegular = new CustomFont();
            _fontImpactRegular.LoadFromXml(_content, "Content/SONICORCA/FONTS/IMPACT/REGULAR_FONT");

            _fontImpactItalic = new CustomFont();
            _fontImpactItalic.LoadFromXml(_content, "Content/SONICORCA/FONTS/IMPACT/ITALIC_FONT");

            _selectionMarkerTexture = _content.Load<Texture2D>("SONICORCA/TITLE/SELECTIONMARKER");
            _leftArrowTexture = _content.Load<Texture2D>("SONICORCA/MENU/LEFT");
            _rightArrowTexture = _content.Load<Texture2D>("SONICORCA/MENU/RIGHT");
            _zigzagTexture = _content.Load<Texture2D>("SONICORCA/TITLE/ZIGZAG");

            _whiteTexture = new Texture2D(_graphicsDevice, 1, 1);
            _whiteTexture.SetData(new[] { Color.White });

            var titleAniGroup = new AnimationGroup();
            titleAniGroup.LoadFromXml(_content, "Content/SONICORCA/TITLE/ANIGROUP");
            
            _miniSonicAniInstance = new AnimationInstance(titleAniGroup, 11);
            _miniTailsAniInstance = new AnimationInstance(titleAniGroup, 13);
            
            _miniSonicAniInstance.Play();
            _miniTailsAniInstance.Play();
        }

        private void InitializeLevelSelect()
        {

        }

        public void Reset()
        {
            _ticks = 0;
            _pressStartActive = true;
            _pressStartScale = new Vector2(1.0f);
            _pressStartOpacity = 1.0;
            _pressStartWhiteAdditive = 0.0;
            _textWhiteAdditive = 0.0;
            _selectionIndex = 0;
            _levelSelectInputState = 0;
            _levelSelectEnabled = false;
            _levelSelectSelectionIndex = 0;
            _demoTimeout = 720;
            _characterSelectTimer = 60;
            InitializeMenuItemWidgets();
            
            _previousKeyboardState = Keyboard.GetState();
            _previousGamePadState = GamePad.GetState(PlayerIndex.One);
        }

        private void InitializeMenuItemWidgets()
        {
            _menuItemWidgets = new MenuItemWidget[5];
            for (int i = -2; i <= 2; i++)
            {
                int menuIndex = (_selectionIndex + i + _menuItems.Length) % _menuItems.Length;
                _menuItemWidgets[i + 2] = new MenuItemWidget
                {
                    MenuItemIndex = menuIndex,
                    OriginOffset = i,
                    X = 960 + 400 * i,
                    Scale = new Vector2(1.0f),
                    Opacity = (float)MenuItemOpacityEaseTimeline.GetValueAt((int)(960 + 400 * i))
                };
            }
            SetSelectionMarkerPositions();
        }
        
        
        private float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
        
        private void SetSelectionMarkerPositions()
        {
            var centerWidget = _menuItemWidgets.First(w => w.OriginOffset == 0);
            int markerOffset = GetMarkerOffset(_selectionIndex);
            _selectedMenuItemMarkerPositions[0] = new Vector2(centerWidget.X - markerOffset - 30, 915);
            _selectedMenuItemMarkerPositions[1] = new Vector2(centerWidget.X + markerOffset - 10, 915);
        }
        
        private int GetMenuItemWidth(int index)
        {
            return (int)_fontImpactRegular.MeasureString(_menuItems[index]).Width;
        }
        
        private int GetMarkerOffset(int index)
        {
            return GetMenuItemWidth(index) / 2 + 30;
        }

        public void Update()
        {
            if (!Visible) return;

            if (_characterSelectActive)
            {
                _miniSonicAniInstance.Animate();
                _miniTailsAniInstance.Animate();
                _characterSelectOpacity = Math.Min(1.0, _characterSelectOpacity + 0.06666666666666667);
                _textOpacity = 1.0 - _characterSelectOpacity;
            }
            else
            {
                _characterSelectOpacity = Math.Max(0.0, _characterSelectOpacity - 0.06666666666666667);
                _textOpacity = 1.0 - _characterSelectOpacity;
            }

            HandleInput();
            _effectEventManager.Update();
            _ticks++;

            if (_demoTimeout != null)
            {
                _demoTimeout--;
                if (_demoTimeout <= 0)
                {
                    _demoTimeout = null;
                    StartDemo();
                }
            }

            if (_characterSelected)
            {
                _characterSelectTimer--;
                if (_characterSelectTimer == 0)
                {
                    OnSelectCharacter();
                }
            }
        }

        private void HandleInput()
        {
            if (_busy) return;

            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            if (!_levelSelectEnabled)
            {


            }

            if (_levelSelectEnabled)
            {
                HandleLevelSelectInput(keyboardState, gamePadState);
            }
            else if (_pressStartActive)
            {
                if (IsKeyJustPressed(keyboardState, Keys.Enter) || IsButtonJustPressed(gamePadState, Buttons.Start))
                {
                    EffectPressStart();
                    _pressStartActive = false;
                    _demoTimeout = null;
                    _audioManager.PlaySound("NAVIGATE/YES");
                }
            }
            else if (_characterSelectActive)
            {
                HandleCharacterSelectInput(keyboardState, gamePadState);
            }
            else
            {
                HandleMenuInput(keyboardState, gamePadState);
            }
            
            _previousKeyboardState = keyboardState;
            _previousGamePadState = gamePadState;
        }
        
        private bool IsKeyJustPressed(KeyboardState current, Keys key)
        {
            return current.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
        }
        
        private bool IsButtonJustPressed(GamePadState current, Buttons button)
        {
            return current.IsButtonDown(button) && !_previousGamePadState.IsButtonDown(button);
        }
        
        private bool IsDPadJustPressed(GamePadState current, ButtonState direction)
        {
            return current.DPad.Left == ButtonState.Pressed && _previousGamePadState.DPad.Left != ButtonState.Pressed;
        }

        private void HandleLevelSelectInput(KeyboardState keyboardState, GamePadState gamePadState)
        {
            if (IsKeyJustPressed(keyboardState, Keys.Up) || (gamePadState.DPad.Up == ButtonState.Pressed && _previousGamePadState.DPad.Up != ButtonState.Pressed))
            {
                _levelSelectSelectionIndex = (_levelSelectSelectionIndex - 1 + _levelSelectItems.Length) % _levelSelectItems.Length;
            }
            else if (IsKeyJustPressed(keyboardState, Keys.Down) || (gamePadState.DPad.Down == ButtonState.Pressed && _previousGamePadState.DPad.Down != ButtonState.Pressed))
            {
                _levelSelectSelectionIndex = (_levelSelectSelectionIndex + 1) % _levelSelectItems.Length;
            }

            if (IsKeyJustPressed(keyboardState, Keys.Enter) || IsButtonJustPressed(gamePadState, Buttons.Start))
            {
                OnLevelSelectStart();
            }

            if (IsKeyJustPressed(keyboardState, Keys.Escape) || IsButtonJustPressed(gamePadState, Buttons.Back))
            {
                _levelSelectEnabled = false;
            }
        }

        private void HandleCharacterSelectInput(KeyboardState keyboardState, GamePadState gamePadState)
        {
            if (IsKeyJustPressed(keyboardState, Keys.Left) || (gamePadState.DPad.Left == ButtonState.Pressed && _previousGamePadState.DPad.Left != ButtonState.Pressed))
            {
                _characterSelectionIndex = (_characterSelectionIndex - 1 + 3) % 3;
                _miniSonicAniInstance.Index = 11;
                _miniTailsAniInstance.Index = 13;
                _audioManager.PlaySound("NAVIGATE/CURSOR");
            }
            else if (IsKeyJustPressed(keyboardState, Keys.Right) || (gamePadState.DPad.Right == ButtonState.Pressed && _previousGamePadState.DPad.Right != ButtonState.Pressed))
            {
                _characterSelectionIndex = (_characterSelectionIndex + 1) % 3;
                _miniSonicAniInstance.Index = 11;
                _miniTailsAniInstance.Index = 13;
                _audioManager.PlaySound("NAVIGATE/CURSOR");
            }

            if (IsKeyJustPressed(keyboardState, Keys.Escape) || IsButtonJustPressed(gamePadState, Buttons.Back))
            {
                _characterSelectActive = false;
                _audioManager.PlaySound("NAVIGATE/BACK");
            }

            if (IsKeyJustPressed(keyboardState, Keys.Enter) || IsButtonJustPressed(gamePadState, Buttons.Start))
            {
                _miniSonicAniInstance.Index = IsSonicActive ? 12 : 11;
                _miniTailsAniInstance.Index = IsTailsActive ? 14 : 13;
                _characterSelected = true;
                _audioManager.PlaySound("NAVIGATE/YES");
            }
        }

        private void HandleMenuInput(KeyboardState keyboardState, GamePadState gamePadState)
        {
            if (IsKeyJustPressed(keyboardState, Keys.Escape) || IsButtonJustPressed(gamePadState, Buttons.Back))
            {
                _demoTimeout = 720;
                _pressStartActive = true;
                _pressStartOpacity = 1.0;
                _pressStartScale = new Vector2(1.0f);
                _pressStartWhiteAdditive = 0.0;
                _selectionIndex = 0;
                InitializeMenuItemWidgets();
                _audioManager.PlaySound("NAVIGATE/BACK");
            }

            if (IsKeyJustPressed(keyboardState, Keys.Left) || IsDPadJustPressed(gamePadState, ButtonState.Pressed))
            {
                _selectionIndex = (_selectionIndex - 1 + _menuItems.Length) % _menuItems.Length;
                EffectNavigateMenu(-1);
                _audioManager.PlaySound("NAVIGATE/CURSOR");
            }
            else if (IsKeyJustPressed(keyboardState, Keys.Right) || (gamePadState.DPad.Right == ButtonState.Pressed && _previousGamePadState.DPad.Right != ButtonState.Pressed))
            {
                _selectionIndex = (_selectionIndex + 1) % _menuItems.Length;
                EffectNavigateMenu(1);
                _audioManager.PlaySound("NAVIGATE/CURSOR");
            }

            if (IsKeyJustPressed(keyboardState, Keys.Enter) || IsButtonJustPressed(gamePadState, Buttons.Start))
            {
                OnSelectMenuItem();
                _audioManager.PlaySound("NAVIGATE/YES");
            }
        }

        private void EffectPressStart()
        {
            _pressStartActive = false;
            _pressStartOpacity = 0.0;
            _pressStartScale = new Vector2(1.2f);
            _pressStartWhiteAdditive = 1.0;
        }

        private void EffectNavigateMenu(int direction)
        {
            _effectEventManager.BeginEvent(EffectNavigateMenuCoroutine(direction));
        }
        
        private IEnumerable<UpdateResult> EffectNavigateMenuCoroutine(int direction)
        {
            _busy = true;
            int oldSelectionIndex = (_selectionIndex - direction + _menuItems.Length) % _menuItems.Length;
            int newSelectionIndex = _selectionIndex;
            
            int oldMarkerOffset = GetMarkerOffset(oldSelectionIndex);
            int newMarkerOffset = GetMarkerOffset(newSelectionIndex);
            
            double markerVelocityX = (newMarkerOffset - oldMarkerOffset) / 7.0;
            double markerVelocityY = 7.0;
            float velocity = direction * -1 * 26.666666f;
            
            float markerStartX1 = 960 - oldMarkerOffset;
            float markerStartX2 = 960 + oldMarkerOffset;
            float markerEndX1 = 960 - newMarkerOffset; 
            float markerEndX2 = 960 + newMarkerOffset;
            
            float markerVelocityX1 = (markerEndX1 - markerStartX1) / 15.0f;
            float markerVelocityX2 = (markerEndX2 - markerStartX2) / 15.0f;
            
            _selectedMenuItemMarkerPositions[0] = new Vector2(markerStartX1, 900);
            _selectedMenuItemMarkerPositions[1] = new Vector2(markerStartX2, 900);
            
            for (int t = 0; t < 15; t++)
            {
                foreach (MenuItemWidget widget in _menuItemWidgets)
                {
                    widget.X += velocity;
                    widget.Opacity = (float)MenuItemOpacityEaseTimeline.GetValueAt((int)widget.X);
                }
                
                if (t <= 7)
                {
                    for (int j = 0; j < _selectedMenuItemMarkerPositions.Length; j++)
                    {
                        _selectedMenuItemMarkerPositions[j].Y -= (float)markerVelocityY;
                    }
                }
                else
                {
                    float progress = (t - 7) / 7.0f;
                    _selectedMenuItemMarkerPositions[0].X = Lerp(markerStartX1, markerEndX1, progress);
                    _selectedMenuItemMarkerPositions[1].X = Lerp(markerStartX2, markerEndX2, progress);
                    
                    for (int j = 0; j < _selectedMenuItemMarkerPositions.Length; j++)
                    {
                        _selectedMenuItemMarkerPositions[j].Y += (float)markerVelocityY;
                    }
                }
                
                yield return UpdateResult.Next;
            }
            
            foreach (MenuItemWidget widget in _menuItemWidgets)
            {
                int newOriginOffset = widget.OriginOffset - direction;
                if (newOriginOffset == -3)
                {
                    newOriginOffset = 2;
                    widget.MenuItemIndex = NegMod(widget.MenuItemIndex - 1, _menuItems.Length);
                }
                else if (newOriginOffset == 3)
                {
                    newOriginOffset = -2;
                    widget.MenuItemIndex = NegMod(widget.MenuItemIndex + 1, _menuItems.Length);
                }
                widget.X = 960 + 400 * newOriginOffset;
                widget.Opacity = (float)MenuItemOpacityEaseTimeline.GetValueAt((int)widget.X);
                widget.OriginOffset = newOriginOffset;
            }
            
            SetSelectionMarkerPositions();
            _busy = false;
        }
        
        private static int NegMod(int x, int divisor)
        {
            while (x < 0)
            {
                x += divisor;
            }
            return x % divisor;
        }

        private void OnSelectMenuItem()
        {
            switch (_selectionIndex)
            {
                case 0: // NEW GAME
                    _characterSelectActive = true;
                    break;
                case 1: // OPTIONS
                    break;
                case 2: // QUIT
                    _busy = true;
                    _effectEventManager.BeginEvent(EffectFadeOut());
                    _titleGameState.Result = TitleGameState.ResultType.Quit;
                    break;
            }
        }

        private void OnSelectCharacter()
        {

            _characterSelected = false;
            _characterSelectActive = false;
        }

        private void OnLevelSelectStart()
        {

        }

        private void StartDemo()
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible) return;


            if (_pressStartActive)
            {
                Color pressStartColor = new Color((float)_pressStartOpacity, 1.0f, 1.0f, 1.0f);
                Vector2 pressStartPosition = new Vector2(960, 900);
                
                Color shadowColor = new Color(0, 0, 0, (float)_pressStartOpacity * 0.5f);
                _fontImpactItalic.DrawString(spriteBatch, "PRESS START", pressStartPosition + new Vector2(2, 2), shadowColor, -1, true);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                Color glowColor = new Color(1.0f, 1.0f, 1.0f, (float)_pressStartOpacity * 0.3f);
                _fontImpactItalic.DrawString(spriteBatch, "PRESS START", pressStartPosition, glowColor, -1, true);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);                
                _fontImpactItalic.DrawString(spriteBatch, "PRESS START", pressStartPosition, pressStartColor, -1, true);

                if (_pressStartOpacity > 0.0)
                {
                    Rectangle textBounds = _fontImpactItalic.MeasureString("PRESS START");
                    int textWidth = textBounds.Width;
                    int zigzagWidth = 79;
                    int spacing = 16;
                    
                    int zigzagOffsetX = textWidth / 2 + spacing + zigzagWidth / 2;
                    int zigzagTextureWidth = _zigzagTexture.Width;
                    int animationOffset = _ticks / 2;
                    int wrapOffsetX1 = zigzagTextureWidth - animationOffset % zigzagTextureWidth;
                    int wrapOffsetX2 = animationOffset % zigzagTextureWidth;
                    
                    Rectangle leftZigzagRect = new Rectangle(
                        (int)(pressStartPosition.X - zigzagOffsetX - zigzagWidth / 2), 
                        932, 
                        zigzagWidth, 
                        0
                    );
                    Rectangle rightZigzagRect = new Rectangle(
                        (int)(pressStartPosition.X + zigzagOffsetX - zigzagWidth / 2), 
                        932, 
                        zigzagWidth, 
                        0
                    );
                    
                    DrawZigZag(spriteBatch, leftZigzagRect, wrapOffsetX1);
                    DrawZigZag(spriteBatch, rightZigzagRect, wrapOffsetX2);
                }
            }


            if (!_pressStartActive && !_characterSelectActive && !_levelSelectEnabled)
            {
                DrawMenuItems(spriteBatch);
            }


            if (_characterSelectActive)
            {
                DrawCharacterSelect(spriteBatch);
            }
        }

        private void DrawMenuItems(SpriteBatch spriteBatch)
        {
            int y = 900;
            foreach (MenuItemWidget widget in _menuItemWidgets)
            {
                string text = _menuItems[widget.MenuItemIndex];
                bool selected = widget.MenuItemIndex == _selectionIndex;
                DrawMenuItem(spriteBatch, text, new Vector2(widget.X, y), widget.Opacity, widget.Scale, selected);
            }
            
            Color markerColor = new Color((float)_textOpacity, 1.0f, 1.0f, 1.0f);
            foreach (Vector2 markerPos in _selectedMenuItemMarkerPositions)
            {
                spriteBatch.Draw(_selectionMarkerTexture, markerPos, markerColor);
            }
        }
        
        private void DrawMenuItem(SpriteBatch spriteBatch, string text, Vector2 position, float opacity, Vector2 scale, bool selected = false)
        {
            if (opacity <= 0.0f)
                return;
                
            int overlay = -1;
            
            if (_textOpacity < 1.0)
            {
                opacity *= (float)_textOpacity;
            }
            
            Color color = new Color(opacity, 1.0f, 1.0f, 1.0f);
            _fontImpactRegular.DrawString(spriteBatch, text, position, color, overlay, true);
        }

        private void DrawCharacterSelect(SpriteBatch spriteBatch)
        {
            double characterSelectOpacity = _characterSelectOpacity;
            Color inactiveColor = new Color((float)characterSelectOpacity * 0.25f, (float)characterSelectOpacity * 0.25f, (float)characterSelectOpacity * 0.25f, (float)characterSelectOpacity);
            Color activeColor = new Color((float)characterSelectOpacity, (float)characterSelectOpacity, (float)characterSelectOpacity, (float)characterSelectOpacity);
            
            string[] characterNames = { "SONIC & TAILS", "SONIC", "TAILS" };
            string text = characterNames[_characterSelectionIndex];
            
            Rectangle backgroundRect = new Rectangle(0, 950, 1920, 60);
            Color backgroundColor = new Color(0.3f * (float)characterSelectOpacity, 0, 0, 0);
            spriteBatch.Draw(_whiteTexture, backgroundRect, backgroundColor);
            
            Vector2 textPosition = new Vector2(960, 950);
            Color textColor = new Color((float)characterSelectOpacity, (float)characterSelectOpacity, (float)characterSelectOpacity, (float)characterSelectOpacity);
            _fontImpactRegular.DrawString(spriteBatch, text, textPosition, textColor, -1, true);
            
            Rectangle textBounds = _fontImpactRegular.MeasureString(text);
            float textHalfWidth = textBounds.Width / 2;
            
            Vector2 leftArrowPos = new Vector2(960 - textHalfWidth - 50, 950);
            Vector2 rightArrowPos = new Vector2(960 + textHalfWidth + -5, 950);
            Color arrowColor = new Color((float)characterSelectOpacity, (float)characterSelectOpacity, (float)characterSelectOpacity, (float)characterSelectOpacity);
            spriteBatch.Draw(_leftArrowTexture, leftArrowPos, arrowColor);
            spriteBatch.Draw(_rightArrowTexture, rightArrowPos, arrowColor);
            
            Color sonicColor = IsSonicActive ? activeColor : inactiveColor;
            Color tailsColor = IsTailsActive ? activeColor : inactiveColor;
            
            _miniSonicAniInstance.Draw(spriteBatch, sonicColor, new Vector2(910, 880));
            _miniTailsAniInstance.Draw(spriteBatch, tailsColor, new Vector2(1010, 880));
        }

        private void DrawZigZag(SpriteBatch spriteBatch, Rectangle rect, int wrapOffsetX)
        {
            rect.Y -= _zigzagTexture.Height / 2;
            rect.Height = _zigzagTexture.Height;

            Rectangle sourceRect = new Rectangle(0, 0, _zigzagTexture.Width, _zigzagTexture.Height);
            Rectangle destRect = rect;
            destRect.X = rect.X - wrapOffsetX;
            destRect.Width = _zigzagTexture.Width;

            while (destRect.X < rect.Right)
            {
                if (destRect.X < rect.Right && destRect.Right > rect.X)
                {
                    Rectangle clippedDest = destRect;
                    Rectangle clippedSource = sourceRect;
                    
                    if (clippedDest.X < rect.X)
                    {
                        int clipAmount = rect.X - clippedDest.X;
                        clippedDest.X = rect.X;
                        clippedDest.Width -= clipAmount;
                        clippedSource.X += clipAmount;
                        clippedSource.Width -= clipAmount;
                    }
                    if (clippedDest.Right > rect.Right)
                    {
                        int clipAmount = clippedDest.Right - rect.Right;
                        clippedDest.Width -= clipAmount;
                        clippedSource.Width -= clipAmount;
                    }
                    
                    if (clippedDest.Width > 0 && clippedSource.Width > 0)
                    {
                        spriteBatch.Draw(_zigzagTexture, clippedDest, clippedSource, Color.White);
                    }
                }
                destRect.X += destRect.Width;
            }
        }

        private IEnumerable<UpdateResult> EffectFadeOut()
        {
            var selectedWidget = _menuItemWidgets.First(w => w.MenuItemIndex == _selectionIndex);
            _titleGameState.Background.WipeOut();
            
            for (int t = 0; t <= 30; t++)
            {
                if (t == 15)
                {
                    _titleGameState.FadeOut();
                }
                selectedWidget.Scale = new Vector2((float)ActivatedTextScaleTimeline.GetValueAt(t));
                _textOpacity = ActivatedTextOpacityTimeline.GetValueAt(t);
                yield return UpdateResult.Next;
            }
        }

        private static readonly EaseTimeline ActivatedTextScaleTimeline = new EaseTimeline(new EaseTimeline.Entry[]
        {
            new EaseTimeline.Entry(0, 1.0),
            new EaseTimeline.Entry(15, 0.8),
            new EaseTimeline.Entry(30, 1.4)
        });

        private static readonly EaseTimeline ActivatedTextOpacityTimeline = new EaseTimeline(new EaseTimeline.Entry[]
        {
            new EaseTimeline.Entry(15, 1.0),
            new EaseTimeline.Entry(30, 0.0)
        });

        private static readonly EaseTimeline MenuItemOpacityEaseTimeline = new EaseTimeline(new EaseTimeline.Entry[]
        {
            new EaseTimeline.Entry(160, 0.0),
            new EaseTimeline.Entry(560, 0.5),
            new EaseTimeline.Entry(960, 1.0),
            new EaseTimeline.Entry(1360, 0.5),
            new EaseTimeline.Entry(1760, 0.0)
        });
    }

    public class MenuItemWidget
    {
        public int OriginOffset { get; set; }
        public int MenuItemIndex { get; set; }
        public float Opacity { get; set; }
        public float X { get; set; }
        public Vector2 Scale { get; set; }
    }
}
