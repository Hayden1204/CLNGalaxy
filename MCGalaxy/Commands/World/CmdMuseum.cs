/*
    Copyright 2011 MCForge
        
    Dual-licensed under the Educational Community License, Version 2.0 and
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
using System.IO;
using System.Threading;
using MCGalaxy.Levels.IO;

namespace MCGalaxy.Commands.World {
    public sealed class CmdMuseum : Command {
        public override string name { get { return "museum"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return CommandTypes.World; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }
        public CmdMuseum() { }

        public override void Use(Player p, string message) {
            if (Player.IsSuper(p)) { MessageInGameOnly(p); return; }
            string[] args = message.Split(' ');
            string path = args.Length == 1 ? LevelInfo.MapPath(args[0]) :
                LevelInfo.BackupPath(args[0], args[1]);
            if (!File.Exists(path)) {
                Player.Message(p, "Level or backup could not be found."); return;
            }
            
            string name = null;
            if (args.Length == 1) {
                name = "&cMuseum " + Server.DefaultColor + "(" + args[0] + ")";
            } else {
                name = "&cMuseum " + Server.DefaultColor + "(" + args[0] + " " + args[1] + ")";
            }
            
            if (p.level.name.CaselessEq(name)) {
                Player.Message(p, "You are already in this museum."); return;
            }
            if (Interlocked.CompareExchange(ref p.LoadingMuseum, 1, 0) == 1) {
                Player.Message(p, "You are already loading a museum level."); return;
            }
            
            try {
                JoinMuseum(p, name, args[0].ToLower(), path);
            } finally {
                Interlocked.Exchange(ref p.LoadingMuseum, 0);
            }
        }
        
        static void JoinMuseum(Player p, string name, string mapName, string path) {
            Level lvl = IMapImporter.Formats[0].Read(path, name, false);
            lvl.MapName = mapName;
            SetLevelProps(lvl);
            Level.LoadMetadata(lvl);            
            if (!lvl.CanJoin(p)) return;

            p.Loading = true;
            Entities.DespawnEntities(p);
            Level oldLevel = p.level;
            p.level = lvl;
            p.SendUserMOTD();
            if (!p.SendRawMap(oldLevel, lvl)) return;

            ushort x = (ushort)(lvl.spawnx * 32 + 16);
            ushort y = (ushort)(lvl.spawny * 32 + 32);
            ushort z = (ushort)(lvl.spawnz * 32 + 16);

            Entities.GlobalSpawn(p, x, y, z, lvl.rotx, lvl.roty, true);
            p.ClearBlockchange();

            Chat.MessageWhere("{0} %Swent to the {1}",
                              pl => Entities.CanSee(pl, p), p.ColoredName, lvl.name);
        }
        
        static void SetLevelProps(Level lvl) {
            lvl.setPhysics(0);
            lvl.backedup = true;
            lvl.permissionbuild = LevelPermission.Nobody;

            lvl.jailx = (ushort)(lvl.spawnx * 32);
            lvl.jaily = (ushort)(lvl.spawny * 32);
            lvl.jailz = (ushort)(lvl.spawnz * 32);
            lvl.jailrotx = lvl.rotx; lvl.jailroty = lvl.roty;
        }
        
        public override void Help(Player p) {
            Player.Message(p, "%T/museum [map] [restore]");
            Player.Message(p, "%HAllows you to access a restore of the map entered. Works on unloaded maps");
        }
    }
}
