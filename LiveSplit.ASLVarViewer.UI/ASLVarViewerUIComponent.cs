﻿using System.Globalization;
using LiveSplit.Model;
using LiveSplit.UI.Components;
using LiveSplit.UI;
using LiveSplit.ASL;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using System.Windows.Forms;

namespace LiveSplit.ASLVarViewer.UI
{
    public class ASLVarViewerUIComponent : IComponent
    {
        public string ComponentName
        {
            get { return "ASL Var Viewer" + (string.IsNullOrWhiteSpace(Settings.TextLabel) ? "" : ": " + Settings.TextLabel); }
        }

        public ASLVarViewerSettings Settings { get; set; }

        public IDictionary<string, Action> ContextMenuControls { get; protected set; }
        protected InfoTextComponent InternalComponent;

        private LiveSplitState _state;
        private ASLEngineHook EngineHook { get; set; }

        public ASLVarViewerUIComponent(LiveSplitState state)
        {
            _state = state;
            this.EngineHook = new ASLEngineHook(state);

            this.Settings = new ASLVarViewerSettings(this.EngineHook);
            this.ContextMenuControls = new Dictionary<String, Action>();
            this.Settings.ASLVarViewerLayoutChanged += Settings_ASLVarViewerLayoutChanged;

            _state.OnStart += _state_OnStart;
            _state.OnReset += state_OnReset;
        }

        void Settings_ASLVarViewerLayoutChanged(object sender, EventArgs e)
        {
            if (this.InternalComponent != null)
            {
                this.InternalComponent.Dispose();
                this.InternalComponent = null;
            }
            this.InternalComponent = new InfoTextComponent(Settings.TextLabel, "0");
        }

        void _state_OnStart(object sender, EventArgs e)
        {
            this.EngineHook.AttemptLoad(); // Connect to ASL
            Settings_ASLVarViewerLayoutChanged(null, null);
        }

        public void Dispose()
        {
            _state.OnReset -= state_OnReset;
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (this.EngineHook.IsLoaded)
            {
                string lives;
                if (Settings.ValueLocation == ASLVarViewerSettings.ValueBucket.CurrentState)
                {
                    lives = this.EngineHook.GetStateValue(Settings.ValueSource);
                }
                else
                {
                    lives = this.EngineHook.GetVariableValue(Settings.ValueSource);
                }

                if (invalidator != null && this.InternalComponent.InformationValue != lives)
                {
                    this.InternalComponent.InformationValue = lives;
                    invalidator.Invalidate(0f, 0f, width, height);
                }
            }
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region region)
        {
            this.PrepareDraw(state);
            this.InternalComponent.DrawVertical(g, state, width, region);
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region region)
        {
            this.PrepareDraw(state);
            this.InternalComponent.DrawHorizontal(g, state, height, region);
        }

        void PrepareDraw(LiveSplitState state)
        {
            this.InternalComponent.NameLabel.ForeColor = state.LayoutSettings.TextColor;
            this.InternalComponent.ValueLabel.ForeColor = state.LayoutSettings.TextColor;
            this.InternalComponent.NameLabel.HasShadow
                = this.InternalComponent.ValueLabel.HasShadow
                = state.LayoutSettings.DropShadows;
        }

        void state_OnReset(object sender, TimerPhase t)
        {
            this.EngineHook.Unload();
        }

        public XmlNode GetSettings(XmlDocument document) { return Settings.GetSettings(document); }

        public Control GetSettingsControl(LayoutMode mode) 
        {
            this.Settings.Mode = mode;
            return this.Settings; 
        }
        
        public void SetSettings(XmlNode settings) { Settings.SetSettings(settings); }

        public void RenameComparison(string oldName, string newName) { }
        public float MinimumWidth { get { return this.InternalComponent.MinimumWidth; } }
        public float MinimumHeight { get { return this.InternalComponent.MinimumHeight; } }
        public float VerticalHeight { get { return this.InternalComponent.VerticalHeight; } }
        public float HorizontalWidth { get { return this.InternalComponent.HorizontalWidth; } }
        public float PaddingLeft { get { return this.InternalComponent.PaddingLeft; } }
        public float PaddingRight { get { return this.InternalComponent.PaddingRight; } }
        public float PaddingTop { get { return this.InternalComponent.PaddingTop; } }
        public float PaddingBottom { get { return this.InternalComponent.PaddingBottom; } }
    }
}
