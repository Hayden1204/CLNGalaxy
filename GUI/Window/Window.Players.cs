﻿/*    
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCGalaxy)
    
    Dual-licensed under the    Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    http://www.opensource.org/licenses/ecl2.php
    http://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
*/
using System;
using System.Windows.Forms;
using MCGalaxy.UI;

namespace MCGalaxy.Gui {
    public partial class Window : Form {
        PlayerProperties playerProps;
         
        void UpdatePlayers() {
            RunOnUI_Async(
                delegate {
                    pl_listBox.Items.Clear();
                    UpdateNotifyIconText();
                    
                    Player[] players = PlayerInfo.Online.Items;
                    foreach (Player p in players)
                        pl_listBox.Items.Add(p.name);
                    
                    if (curPlayer == null) return;
                    if (PlayerInfo.FindExact(curPlayer.name) != null) return;
                    
                    curPlayer = null;
                    playerProps = null;
                    pl_gbProps.Text = "Properties for (none selected)";
                    pl_pgProps.SelectedObject = null;
                });
        }
        
        void AppendPlayerStatus(string text) {
            if (InvokeRequired) {
                Action<string> d = AppendPlayerStatus;
                Invoke(d, new object[] { text, true });
            } else {
                pl_statusBox.AppendText(text + Environment.NewLine);
            }
        }
        
        void LoadPlayerTabDetails(object sender, EventArgs e) {
            Player p = PlayerInfo.FindExact(pl_listBox.Text);
            if (p == null || p == curPlayer) return;
            
            pl_statusBox.Text = "";
            AppendPlayerStatus("==" + p.name + "==");
            playerProps = new PlayerProperties(p);
            pl_gbProps.Text = "Properties for " + p.name;
            pl_pgProps.SelectedObject = playerProps;
            curPlayer = p;
            
            UpdatePlayerSelected();
        }

        void UpdatePlayerSelected() {          
            if (tabs.SelectedTab != tp_Players) return;
            try { pl_pgProps.Refresh(); } catch { }
        }

        void pl_BtnUndo_Click(object sender, EventArgs e) {
            if (curPlayer == null) { AppendPlayerStatus("No player selected"); return; }
            string time = pl_txtUndo.Text.Trim();
            if (time.Length == 0) { AppendPlayerStatus("Amount of time to undo required"); return; }

            UIHelpers.HandleCommand("UndoPlayer " + curPlayer.name + " " + time);
            AppendPlayerStatus("Undid player for " + time + " seconds");
        }

        void pl_BtnMessage_Click(object sender, EventArgs e) {
            if (curPlayer == null) { AppendPlayerStatus("No player selected"); return; }
            string text = pl_txtMessage.Text.Trim();
            if (text.Length == 0) { AppendPlayerStatus("No message to send"); return; }
            
            Player.Message(curPlayer, "<CONSOLE>: &f" + pl_txtMessage.Text);            
            AppendPlayerStatus("Sent player message: " + pl_txtMessage.Text);
            pl_txtMessage.Text = "";
        }

        void pl_BtnSendCommand_Click(object sender, EventArgs e) {
            if (curPlayer == null) { AppendPlayerStatus("No player selected"); return; }
            string text = pl_txtImpersonate.Text.Trim();
            if (text.Length == 0) { AppendPlayerStatus("No command to execute"); return; }
            
            string[] args = text.SplitSpaces(2);
            string cmdName = args[0], cmdArgs = args.Length > 1 ? args[1] : "";
            curPlayer.HandleCommand(cmdName, cmdArgs);
                
            if (args.Length > 1) {
                AppendPlayerStatus("Made player do /" + cmdName + " " + cmdArgs);
            } else {
                AppendPlayerStatus("Made player do /" + cmdName);
            }
            pl_txtImpersonate.Text = "";
        }

        void pl_BtnSlap_Click(object sender, EventArgs e) {  DoCmd("slap", "Slapped"); }
        void pl_BtnKill_Click(object sender, EventArgs e) {  DoCmd("kill", "Killed"); }
        void pl_BtnWarn_Click(object sender, EventArgs e) {  DoCmd("warn", "Warned"); }
        void pl_BtnKick_Click(object sender, EventArgs e) {  DoCmd("kick", "Kicked"); }
        void pl_BtnBan_Click(object sender, EventArgs e) {   DoCmd("ban", "Banned"); }
        void pl_BtnIPBan_Click(object sender, EventArgs e) { DoCmd("banip", "IP-Banned"); }
        
        void DoCmd(string cmdName, string action) {
            if (curPlayer == null) { AppendPlayerStatus("No player selected"); return; }
            UIHelpers.HandleCommand(cmdName + " " + curPlayer.name);
            AppendPlayerStatus(action + " player");
        }

        void pl_BtnRules_Click(object sender, EventArgs e) {
            if (curPlayer == null) { AppendPlayerStatus("No player selected"); return; }
            UIHelpers.HandleCommand("Rules " + curPlayer.name);
            AppendPlayerStatus("Sent rules to player");
        }

        void pl_BtnSpawn_Click(object sender, EventArgs e) {
            if (curPlayer == null) { AppendPlayerStatus("No player selected"); return; }
            curPlayer.HandleCommand("Spawn", "");
            AppendPlayerStatus("Sent player to spawn");
        }

        void pl_listBox_Click(object sender, EventArgs e) {
            LoadPlayerTabDetails(sender, e);
        }

        void pl_txtImpersonate_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) pl_BtnSendCommand_Click(sender, e);
        }
        void pl_txtUndo_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) pl_BtnUndo_Click(sender, e);
        }
        void pl_txtMessage_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) pl_BtnMessage_Click(sender, e);
        }
    }
}
