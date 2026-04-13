using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;
using PEPCO.Utilities;

namespace PEPCO
{
    public static class ScriptHelpers
    {
        #region API & Mod Detection Checks

        /// <summary>
        /// Checks if Factions API is available and ready.
        /// </summary>
        public static bool AreFactionsAvailable()
        {
            return MyAPIGateway.Session?.Factions != null;
        }

        /// <summary>
        /// Checks if Utilities API is available and ready.
        /// </summary>
        public static bool AreUtilitiesAvailable()
        {
            return MyAPIGateway.Utilities != null;
        }

        /// <summary>
        /// Safely gets the current session with a null check.
        /// Returns null if the session is not available.
        /// </summary>
        public static IMySession GetSessionSafe()
        {
            return MyAPIGateway.Session;
        }

        /// <summary>
        /// Checks if the GPS API is available and ready.
        /// </summary>
        public static bool IsGpsAvailable()
        {
            return MyAPIGateway.Session?.GPS != null;
        }

        /// <summary>
        /// Checks if a mod with the specified published file ID is currently loaded.
        /// </summary>
        /// <param name="publishedFileId">The Steam Workshop published file ID of the mod.</param>
        /// <param name="modName">A friendly name used in the log message.</param>
        /// <returns>True if the mod is detected; otherwise, false.</returns>
        public static bool IsModDetected(ulong publishedFileId, string modName)
        {
            if (MyAPIGateway.Session?.Mods == null) return false;

            foreach (var mod in MyAPIGateway.Session.Mods)
            {
                if (mod.PublishedFileId == publishedFileId)
                {
                    Log.Info($"{modName} Detected: {mod.Name} ({mod.PublishedFileId})");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the Multiplayer API is available and ready.
        /// </summary>
        public static bool IsMultiplayerAvailable()
        {
            return MyAPIGateway.Multiplayer != null;
        }

        #endregion

        #region Color Helpers

        /// <summary>
        /// Converts a Color object to its hex string representation.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <param name="includeAlpha">If true, returns an 8-character ARGB hex string instead of 6-character RGB.</param>
        /// <returns>The hex string representing the color.</returns>
        public static string ColorToHex(Color color, bool includeAlpha = false)
        {
            return includeAlpha
                ? $"{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}"
                : $"{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        /// <summary>
        /// Safely converts a Color to a hex string with error handling.
        /// </summary>
        /// <param name="color">Color to convert.</param>
        /// <param name="includeAlpha">Whether to include the alpha channel.</param>
        /// <param name="fallbackHex">Hex string to return if conversion fails.</param>
        /// <returns>Hex string or fallback.</returns>
        public static string ColorToHexSafe(Color color, bool includeAlpha = false, string fallbackHex = "FFFFFF")
        {
            try
            {
                return ColorToHex(color, includeAlpha);
            }
            catch (Exception ex)
            {
                LogError($"ColorToHexSafe: Failed to convert color to hex: {ex.Message}");
                return fallbackHex;
            }
        }

        /// <summary>
        /// Gets the current suit color of a given player. Falls back to Red if it fails.
        /// </summary>
        /// <param name="player">The player object to extract the color from.</param>
        /// <returns>The player's suit Color.</returns>
        public static Color GetPlayerColorSafe(IMyPlayer player)
        {
            if (player?.Identity == null)
                return Color.Red; // Fallback/default color

            try
            {
                var colorMask = player.Identity.ColorMask;
                if (!colorMask.HasValue) return Color.Red;

                var normalHSV = MyColorPickerConstants.HSVOffsetToHSV(colorMask.Value);
                return ColorExtensions.HSVtoColor(normalHSV);
            }
            catch
            {
                // In case something unexpected happens in the conversion
                return Color.Red;
            }
        }

        /// <summary>
        /// Gets a player's suit color based purely on their Identity ID.
        /// </summary>
        /// <param name="identityId">The player's unique identity ID.</param>
        /// <returns>The color of the player's suit, or Magenta on failure.</returns>
        public static Color GetSuitColor(long identityId)
        {
            if (MyAPIGateway.Players == null) return new Color(255, 0, 255);

            var identities = new List<IMyIdentity>();
            MyAPIGateway.Players.GetAllIdentites(identities);

            var identity = identities.FirstOrDefault(id => id != null && id.IdentityId == identityId);

            // Bail out early if no matching identity or missing color mask
            if (identity == null || !identity.ColorMask.HasValue)
                return new Color(255, 0, 255);

            var colorMask = identity.ColorMask.Value;
            var normalHSV = MyColorPickerConstants.HSVOffsetToHSV(colorMask);
            return ColorExtensions.HSVtoColor(normalHSV);
        }

        /// <summary>
        /// Safely converts a hex color string to a Color object, with a specified fallback.
        /// </summary>
        /// <param name="hex">Hex color string (with or without '#').</param>
        /// <param name="fallback">Color to return if the conversion fails.</param>
        /// <returns>The converted Color or the fallback.</returns>
        public static Color HexToColorSafe(string hex, Color fallback)
        {
            var normalized = NormalizeHex(hex);
            if (string.IsNullOrEmpty(normalized))
                return fallback;

            try
            {
                return ColorExtensions.HexToColor(normalized);
            }
            catch (Exception ex)
            {
                LogError($"HexToColorSafe: Failed to convert '{hex}' to color: {ex.Message}");
                return fallback;
            }
        }

        /// <summary>
        /// Cleans a hex string, stripping out invalid characters and ensuring correct length (6 chars).
        /// Drops alpha channel if provided as 8 characters.
        /// </summary>
        /// <param name="hex">The raw hex string.</param>
        /// <returns>A clean 6-character hex string, or null if invalid.</returns>
        public static string NormalizeHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return null;

            // Remove leading '#', trim, uppercase
            hex = hex.Trim();
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            hex = hex.ToUpperInvariant();

            // Accept only 6 or 8 hex digits; prefer 6 without alpha
            if (IsHex(hex) && (hex.Length == 6 || hex.Length == 8))
            {
                // If 8 digits (ARGB/RGBA), convert to 6 by dropping alpha if needed
                if (hex.Length == 8)
                {
                    // Assume AARRGGBB; drop AA -> RRBBGG stays RR GG BB
                    hex = hex.Substring(2, 6);
                }
                return hex;
            }

            return null;
        }

        private static bool IsHex(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                bool isHex = (c >= '0' && c <= '9') ||
                             (c >= 'A' && c <= 'F') ||
                             (c >= 'a' && c <= 'f');
                if (!isHex)
                    return false;
            }
            return true;
        }

        #endregion

        #region Debug Logging

        /// <summary>
        /// Logs a debug message if debugging is enabled via ModParameter.
        /// </summary>
        public static void LogDebug(string message)
        {
            if (ModParameter.IsDebug())
            {
                Log.Info(message);
            }
        }

        /// <summary>
        /// Logs an error message to the log file.
        /// </summary>
        public static void LogError(string message)
        {
            Log.Error(message);
        }

        #endregion

        #region Faction Queries

        /// <summary>
        /// Checks if two players are in the same faction.
        /// Returns false if either player is not in a faction.
        /// </summary>
        public static bool ArePlayersInSameFaction(long identityId1, long identityId2)
        {
            var faction1 = GetPlayerFaction(identityId1);
            var faction2 = GetPlayerFaction(identityId2);

            return faction1 != null && faction2 != null && faction1.FactionId == faction2.FactionId;
        }

        /// <summary>
        /// Gets all factions that are allies with the specified faction.
        /// Returns an empty list if none are found.
        /// Note: This checks relations with all discovered factions.
        /// </summary>
        public static List<IMyFaction> GetFactionAllies(long factionId)
        {
            var allies = new List<IMyFaction>();
            var faction = GetFactionById(factionId);

            if (faction == null || MyAPIGateway.Session?.Factions == null)
                return allies;

            var identities = new List<IMyIdentity>();
            MyAPIGateway.Players.GetAllIdentites(identities);

            var checkedFactions = new HashSet<long> { factionId };

            foreach (var identity in identities)
            {
                if (identity == null) continue;

                var otherFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(identity.IdentityId);
                if (otherFaction != null && checkedFactions.Add(otherFaction.FactionId))
                {
                    var relation = MyAPIGateway.Session.Factions.GetRelationBetweenFactions(factionId, otherFaction.FactionId);
                    if (relation == MyRelationsBetweenFactions.Friends)
                        allies.Add(otherFaction);
                }
            }

            return allies;
        }

        /// <summary>
        /// Gets a faction by its unique ID.
        /// Returns null if not found.
        /// </summary>
        public static IMyFaction GetFactionById(long factionId)
        {
            return MyAPIGateway.Session?.Factions?.TryGetFactionById(factionId);
        }

        /// <summary>
        /// Gets a faction by its name (case-sensitive).
        /// Returns null if not found.
        /// </summary>
        public static IMyFaction GetFactionByName(string name)
        {
            if (string.IsNullOrEmpty(name) || MyAPIGateway.Session?.Factions == null)
                return null;

            var identities = new List<IMyIdentity>();
            MyAPIGateway.Players.GetAllIdentites(identities);

            var checkedFactions = new HashSet<long>();

            foreach (var identity in identities)
            {
                if (identity == null) continue;

                var faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(identity.IdentityId);
                if (faction != null && checkedFactions.Add(faction.FactionId))
                {
                    if (faction.Name == name)
                        return faction;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a faction by its tag (case-sensitive).
        /// Returns null if not found.
        /// </summary>
        public static IMyFaction GetFactionByTag(string tag)
        {
            if (string.IsNullOrEmpty(tag) || MyAPIGateway.Session?.Factions == null)
                return null;

            return MyAPIGateway.Session.Factions.TryGetFactionByTag(tag);
        }

        /// <summary>
        /// Gets all factions that are enemies with the specified faction.
        /// Returns an empty list if none are found.
        /// </summary>
        public static List<IMyFaction> GetFactionEnemies(long factionId)
        {
            var enemies = new List<IMyFaction>();
            var faction = GetFactionById(factionId);

            if (faction == null || MyAPIGateway.Session?.Factions == null)
                return enemies;

            var identities = new List<IMyIdentity>();
            MyAPIGateway.Players.GetAllIdentites(identities);

            var checkedFactions = new HashSet<long> { factionId };

            foreach (var identity in identities)
            {
                if (identity == null) continue;

                var otherFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(identity.IdentityId);
                if (otherFaction != null && checkedFactions.Add(otherFaction.FactionId))
                {
                    var relation = MyAPIGateway.Session.Factions.GetRelationBetweenFactions(factionId, otherFaction.FactionId);
                    if (relation == MyRelationsBetweenFactions.Enemies)
                        enemies.Add(otherFaction);
                }
            }

            return enemies;
        }

        /// <summary>
        /// Gets all members of a faction as a list of Identity IDs.
        /// Returns an empty list if the faction is not found or has no members.
        /// </summary>
        public static List<long> GetFactionMembers(long factionId)
        {
            var members = new List<long>();
            var faction = GetFactionById(factionId);

            if (faction?.Members == null)
                return members;

            foreach (var member in faction.Members)
            {
                members.Add(member.Key);
            }

            return members;
        }

        /// <summary>
        /// Gets all pending join requests for the specified faction.
        /// Returns an empty list if none are found.
        /// </summary>
        public static List<long> GetFactionRequests(long factionId)
        {
            var requests = new List<long>();
            var faction = GetFactionById(factionId);

            if (faction?.JoinRequests == null)
                return requests;

            foreach (var request in faction.JoinRequests)
            {
                requests.Add(request.Key);
            }

            return requests;
        }

        /// <summary>
        /// Gets all factions that the specified faction is at war with.
        /// This is an alias for GetFactionEnemies for semantic clarity.
        /// </summary>
        public static List<IMyFaction> GetFactionWars(long factionId)
        {
            return GetFactionEnemies(factionId);
        }

        /// <summary>
        /// Gets the faction that a player belongs to.
        /// Returns null if the player is not in a faction.
        /// </summary>
        public static IMyFaction GetPlayerFaction(long identityId)
        {
            return MyAPIGateway.Session?.Factions?.TryGetPlayerFaction(identityId);
        }

        /// <summary>
        /// Checks if a player is in the specified faction.
        /// </summary>
        public static bool IsPlayerInFaction(long identityId, long factionId)
        {
            var playerFaction = GetPlayerFaction(identityId);
            return playerFaction != null && playerFaction.FactionId == factionId;
        }

        #endregion

        #region Faction Role Queries

        /// <summary>
        /// Demotes a faction member to the previous rank.
        /// Returns true if successful. Only works server-side or in single-player.
        /// </summary>
        public static bool DemoteMember(long factionId, long identityId)
        {
            if (MyAPIGateway.Session?.Factions == null)
                return false;

            try
            {
                MyAPIGateway.Session.Factions.DemoteMember(factionId, identityId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the identity ID of the founder of the specified faction.
        /// Returns 0 if the faction is not found.
        /// </summary>
        public static long GetFactionFounder(long factionId)
        {
            var faction = GetFactionById(factionId);
            return faction?.FounderId ?? 0;
        }

        /// <summary>
        /// Gets all leaders (Founders or Leaders) in the specified faction.
        /// Returns an empty list if none are found.
        /// </summary>
        public static List<long> GetFactionLeaders(long factionId)
        {
            var leaders = new List<long>();
            var faction = GetFactionById(factionId);

            if (faction?.Members == null)
                return leaders;

            foreach (var memberKvp in faction.Members)
            {
                if (memberKvp.Value.IsLeader || memberKvp.Value.IsFounder)
                    leaders.Add(memberKvp.Key);
            }

            return leaders;
        }

        /// <summary>
        /// Gets all members with the specified role in a faction.
        /// Returns an empty list if none are found.
        /// </summary>
        public static List<long> GetFactionMembersByRole(long factionId, MyPromoteLevel role)
        {
            var members = new List<long>();
            var faction = GetFactionById(factionId);

            if (faction?.Members == null)
                return members;

            foreach (var memberKvp in faction.Members)
            {
                bool matches = false;

                switch (role)
                {
                    case MyPromoteLevel.Owner:
                        matches = memberKvp.Value.IsFounder;
                        break;
                    case MyPromoteLevel.Admin:
                        matches = memberKvp.Value.IsLeader && !memberKvp.Value.IsFounder;
                        break;
                }

                if (matches)
                    members.Add(memberKvp.Key);
            }

            return members;
        }

        /// <summary>
        /// Gets all recruits (regular members) in the specified faction.
        /// Returns an empty list if none are found.
        /// </summary>
        public static List<long> GetFactionRecruits(long factionId)
        {
            var recruits = new List<long>();
            var faction = GetFactionById(factionId);

            if (faction?.Members == null)
                return recruits;

            foreach (var memberKvp in faction.Members)
            {
                if (!memberKvp.Value.IsLeader && !memberKvp.Value.IsFounder)
                    recruits.Add(memberKvp.Key);
            }

            return recruits;
        }

        /// <summary>
        /// Gets the rank/role of a member in their faction.
        /// Returns MyPromoteLevel.None if not found or if they are a standard member.
        /// </summary>
        public static MyPromoteLevel GetMemberRole(long identityId)
        {
            var faction = GetPlayerFaction(identityId);
            if (faction?.Members == null)
                return MyPromoteLevel.None;

            MyFactionMember member;
            if (faction.Members.TryGetValue(identityId, out member))
            {
                if (member.IsFounder) return MyPromoteLevel.Owner;
                if (member.IsLeader) return MyPromoteLevel.Admin;
            }

            return MyPromoteLevel.None;
        }

        /// <summary>
        /// Checks if a player is the founder of their faction.
        /// </summary>
        public static bool IsFounder(long identityId)
        {
            var faction = GetPlayerFaction(identityId);
            return faction != null && faction.FounderId == identityId;
        }

        /// <summary>
        /// Checks if a player is a leader in their faction (Leader or Founder).
        /// </summary>
        public static bool IsLeader(long identityId)
        {
            var faction = GetPlayerFaction(identityId);
            if (faction?.Members == null)
                return false;

            MyFactionMember member;
            if (faction.Members.TryGetValue(identityId, out member))
                return member.IsLeader || member.IsFounder;

            return false;
        }

        /// <summary>
        /// Checks if a player is a standard member in their faction.
        /// </summary>
        public static bool IsMember(long identityId)
        {
            var faction = GetPlayerFaction(identityId);
            return faction?.Members != null && faction.Members.ContainsKey(identityId);
        }

        /// <summary>
        /// Checks if a player is a recruit in their faction (default member without leadership).
        /// </summary>
        public static bool IsRecruit(long identityId)
        {
            var faction = GetPlayerFaction(identityId);
            if (faction?.Members == null)
                return false;

            MyFactionMember member;
            if (faction.Members.TryGetValue(identityId, out member))
                return !member.IsLeader && !member.IsFounder;

            return false;
        }

        /// <summary>
        /// Promotes a faction member to the next rank.
        /// Returns true if successful. Only works server-side or in single-player.
        /// </summary>
        public static bool PromoteMember(long factionId, long identityId)
        {
            if (MyAPIGateway.Session?.Factions == null)
                return false;

            try
            {
                MyAPIGateway.Session.Factions.PromoteMember(factionId, identityId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Transfers leadership of a faction to another member.
        /// Returns true if successful. Only works server-side or in single-player.
        /// </summary>
        public static bool TransferLeadership(long factionId, long newLeaderId)
        {
            if (MyAPIGateway.Session?.Factions == null)
                return false;

            var faction = GetFactionById(factionId);
            if (faction == null || !faction.Members.ContainsKey(newLeaderId))
                return false;

            try
            {
                // Promote the new leader to max rank
                MyAPIGateway.Session.Factions.PromoteMember(factionId, newLeaderId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region GPS Helpers

        /// <summary>
        /// Gets a GPS marker by its hash.
        /// Returns null if not found.
        /// </summary>
        public static IMyGps GetGpsByHash(long identityId, int hash)
        {
            var gpsList = GetPlayerGpsList(identityId);
            return gpsList.FirstOrDefault(g => g.Hash == hash);
        }

        /// <summary>
        /// Gets all GPS markers for the specified player identity.
        /// Returns an empty list if not available.
        /// </summary>
        public static List<IMyGps> GetPlayerGpsList(long identityId)
        {
            if (MyAPIGateway.Session?.GPS == null)
                return new List<IMyGps>();

            return MyAPIGateway.Session.GPS.GetGpsList(identityId);
        }

        /// <summary>
        /// Checks if a GPS marker is currently visible to the player.
        /// </summary>
        public static bool IsGpsVisible(IMyGps gps)
        {
            return gps != null && !gps.DiscardAt.HasValue;
        }

        #endregion

        #region Math & Transforms

        /// <summary>
        /// Transforms a direction vector from world/global space into local space
        /// by applying the inverse of the given world transformation matrix.
        /// </summary>
        /// <param name="globalDirection">The direction vector in world coordinates.</param>
        /// <param name="worldMatrix">The world transformation matrix.</param>
        /// <returns>The direction vector in local coordinates.</returns>
        public static Vector3D GlobalDirectionToLocal(Vector3D globalDirection, MatrixD worldMatrix)
        {
            return Vector3D.TransformNormal(globalDirection, InvertMatrixLight(worldMatrix));
        }

        /// <summary>
        /// Transforms a position from world/global space into local space
        /// by applying the inverse of the given world transformation matrix.
        /// </summary>
        /// <param name="globalPosition">The position vector in world coordinates.</param>
        /// <param name="worldMatrix">The world transformation matrix.</param>
        /// <returns>The position vector in local coordinates.</returns>
        public static Vector3D GlobalPositionToLocal(Vector3D globalPosition, MatrixD worldMatrix)
        {
            return Vector3D.Transform(globalPosition, InvertMatrixLight(worldMatrix));
        }

        /// <summary>
        /// Computes a lightweight inverse of a transformation matrix,
        /// correctly inverting its rotation and translation components
        /// for any orthonormal (pure rotation + translation) matrix.
        /// This ignores scaling/shearing for performance.
        /// </summary>
        /// <param name="matrix">The orthonormal transformation matrix to invert.</param>
        /// <returns>The inverted transformation matrix.</returns>
        public static MatrixD InvertMatrixLight(MatrixD matrix)
        {
            MatrixD inverted = MatrixD.Identity;

            // Transpose the rotation part (upper 3×3) for orthonormal inverse
            inverted.M11 = matrix.M11;
            inverted.M12 = matrix.M21;
            inverted.M13 = matrix.M31;

            inverted.M21 = matrix.M12;
            inverted.M22 = matrix.M22;
            inverted.M23 = matrix.M32;

            inverted.M31 = matrix.M13;
            inverted.M32 = matrix.M23;
            inverted.M33 = matrix.M33;

            // Invert translation: apply inverted rotation to negative original position
            inverted.Translation = -Vector3D.TransformNormal(matrix.Translation, inverted);

            return inverted;
        }

        /// <summary>
        /// Returns true if 'test' lies strictly between 'small' (lower bound) and 'large' (upper bound)
        /// on both X and Y using component-wise comparison.
        /// </summary>
        /// <param name="test">Vector to evaluate.</param>
        /// <param name="small">Lower bound (min X and Y).</param>
        /// <param name="large">Upper bound (max X and Y).</param>
        /// <returns>True when small.X &lt; test.X &lt; large.X and small.Y &lt; test.Y &lt; large.Y.</returns>
        public static bool IsBetween(Vector2 test, Vector2 small, Vector2 large)
        {
            var issmallerThanLarge = large.X > test.X && large.Y > test.Y;
            var islargerThanSmall = small.X < test.X && small.Y < test.Y;

            return issmallerThanLarge && islargerThanSmall;
        }

        /// <summary>
        /// Returns true if v1 is strictly greater than v2 on both components (component-wise comparison).
        /// This does not compare vector magnitudes/lengths.
        /// </summary>
        /// <param name="v1">First vector.</param>
        /// <param name="v2">Second vector.</param>
        /// <returns>True when v1.X > v2.X and v1.Y > v2.Y; otherwise, false.</returns>
        public static bool IsFirstLarger(Vector2 v1, Vector2 v2)
        {
            return v1.X > v2.X && v1.Y > v2.Y;
        }

        /// <summary>
        /// Transforms a direction vector from local space into world/global space
        /// using the given world transformation matrix.
        /// </summary>
        /// <param name="localDirection">The direction vector in local coordinates.</param>
        /// <param name="worldMatrix">The world transformation matrix.</param>
        /// <returns>The direction vector in world coordinates.</returns>
        public static Vector3D LocalDirectionToGlobal(Vector3D localDirection, MatrixD worldMatrix)
        {
            return Vector3D.TransformNormal(localDirection, worldMatrix);
        }

        /// <summary>
        /// Transforms a position from local space into world/global space
        /// using the given world transformation matrix.
        /// </summary>
        /// <param name="localPosition">The position vector in local coordinates.</param>
        /// <param name="worldMatrix">The world transformation matrix.</param>
        /// <returns>The position vector in world coordinates.</returns>
        public static Vector3D LocalPositionToGlobal(Vector3D localPosition, MatrixD worldMatrix)
        {
            return Vector3D.Transform(localPosition, worldMatrix);
        }

        /// <summary>
        /// Converts a global/world-space matrix to its local-space equivalent
        /// relative to a given parent matrix, without performing a full matrix inversion.
        /// This is optimized for orthonormal transforms (rotation + translation).
        /// </summary>
        /// <param name="globalMatrix">The object's global transformation matrix.</param>
        /// <param name="parentMatrix">The parent object's global transformation matrix.</param>
        /// <returns>The object's local transformation matrix relative to the parent.</returns>
        public static MatrixD ToLocalMatrixFast(MatrixD globalMatrix, MatrixD parentMatrix)
        {
            // Extract parent's rotation as a 3x3 (upper-left) and transpose it for inverse
            MatrixD invParentRot = MatrixD.Transpose(parentMatrix);

            // Extract parent's translation
            Vector3D parentPos = parentMatrix.Translation;

            // Build inverse parent matrix (rotation + translation)
            MatrixD invParent = invParentRot;
            invParent.Translation = Vector3D.TransformNormal(-parentPos, invParentRot);

            // Multiply: global → local
            return globalMatrix * invParent;
        }

        #endregion

        #region Player Identity & Steam ID Helpers

        /// <summary>
        /// Gets all identities in the game (including NPC identities).
        /// Returns an empty list if none found.
        /// </summary>
        public static List<IMyIdentity> GetAllIdentities()
        {
            var identities = new List<IMyIdentity>();

            if (MyAPIGateway.Players != null)
                MyAPIGateway.Players.GetAllIdentites(identities);

            return identities;
        }

        /// <summary>
        /// Gets all currently connected players.
        /// Returns an empty list if none found.
        /// </summary>
        public static List<IMyPlayer> GetAllPlayers()
        {
            var players = new List<IMyPlayer>();

            if (MyAPIGateway.Players != null)
                MyAPIGateway.Players.GetPlayers(players);

            return players;
        }

        /// <summary>
        /// Gets the identity associated with the given Steam ID.
        /// Returns null if not found.
        /// </summary>
        public static IMyIdentity GetIdentityBySteamId(ulong steamId)
        {
            if (MyAPIGateway.Players == null)
                return null;

            var identities = new List<IMyIdentity>();
            MyAPIGateway.Players.GetAllIdentites(identities);

            foreach (var identity in identities)
            {
                if (identity == null) continue;

                var player = GetPlayerByIdentityId(identity.IdentityId);
                if (player != null && player.SteamUserId == steamId)
                    return identity;
            }

            return null;
        }

        /// <summary>
        /// Gets the Identity ID of the local player (client-side only).
        /// Returns 0 if not available or on dedicated server.
        /// </summary>
        public static long GetLocalPlayerIdentityId()
        {
            return MyAPIGateway.Session?.Player?.IdentityId ?? 0;
        }

        /// <summary>
        /// Gets the display name of the local player (client-side only).
        /// Returns null if not available or on dedicated server.
        /// </summary>
        public static string GetLocalPlayerName()
        {
            return MyAPIGateway.Session?.Player?.DisplayName;
        }

        /// <summary>
        /// Gets the Steam ID of the local player (client-side only).
        /// Returns 0 if not available or on dedicated server.
        /// </summary>
        public static ulong GetLocalPlayerSteamId()
        {
            if (MyAPIGateway.Session == null || MyAPIGateway.Multiplayer == null)
                return 0;

            return MyAPIGateway.Multiplayer.MyId;
        }

        /// <summary>
        /// Gets all NPC identities (identities without an associated player).
        /// Returns an empty list if none found.
        /// </summary>
        public static List<IMyIdentity> GetNpcIdentities()
        {
            var npcIdentities = new List<IMyIdentity>();

            if (MyAPIGateway.Players == null)
                return npcIdentities;

            var allIdentities = new List<IMyIdentity>();
            MyAPIGateway.Players.GetAllIdentites(allIdentities);

            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var identity in allIdentities)
            {
                if (identity == null) continue;

                bool isPlayerIdentity = players.Any(p => p != null && p.IdentityId == identity.IdentityId);
                if (!isPlayerIdentity)
                    npcIdentities.Add(identity);
            }

            return npcIdentities;
        }

        /// <summary>
        /// Gets the player object associated with the given Identity ID.
        /// Returns null if not found.
        /// </summary>
        public static IMyPlayer GetPlayerByIdentityId(long identityId)
        {
            if (MyAPIGateway.Players == null) return null;

            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            return players.FirstOrDefault(p => p != null && p.IdentityId == identityId);
        }

        /// <summary>
        /// Gets the display name of a player by their Identity ID.
        /// Returns null if not found.
        /// </summary>
        public static string GetPlayerName(long identityId)
        {
            if (MyAPIGateway.Players == null) return null;

            var identities = new List<IMyIdentity>();
            MyAPIGateway.Players.GetAllIdentites(identities);

            var identity = identities.FirstOrDefault(i => i != null && i.IdentityId == identityId);
            return identity?.DisplayName;
        }

        /// <summary>
        /// Gets the Steam ID associated with the given Identity ID.
        /// Returns 0 if not found.
        /// </summary>
        public static ulong GetSteamIdByIdentityId(long identityId)
        {
            var player = GetPlayerByIdentityId(identityId);
            return player?.SteamUserId ?? 0;
        }

        #endregion

        #region Session & World Utilities

        /// <summary>
        /// Gets the current game session object.
        /// Returns null if not available.
        /// </summary>
        public static IMySession GetCurrentSession()
        {
            return MyAPIGateway.Session;
        }

        /// <summary>
        /// Gets the elapsed game time since the world was created.
        /// Returns TimeSpan.Zero if session not available.
        /// </summary>
        public static TimeSpan GetElapsedGameTime()
        {
            if (MyAPIGateway.Session == null)
                return TimeSpan.Zero;

            // 2081-01-01 is the default SE world start date
            return MyAPIGateway.Session.GameDateTime - new DateTime(2081, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        /// <summary>
        /// Gets the count of currently online players.
        /// Returns 0 if unable to determine.
        /// </summary>
        public static int GetOnlinePlayersCount()
        {
            if (MyAPIGateway.Players == null)
                return 0;

            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            return players.Count;
        }

        /// <summary>
        /// Gets the name of the current server/world.
        /// Returns null if session not available.
        /// </summary>
        public static string GetServerName()
        {
            return MyAPIGateway.Session?.Name;
        }

        /// <summary>
        /// Checks if the current instance is a dedicated server.
        /// </summary>
        public static bool IsDedicatedServer()
        {
            return MyAPIGateway.Utilities != null && MyAPIGateway.Utilities.IsDedicated;
        }

        /// <summary>
        /// Checks if the current game is running in multiplayer.
        /// </summary>
        public static bool IsMultiplayer()
        {
            if (MyAPIGateway.Multiplayer == null)
                return false;

            return MyAPIGateway.Multiplayer.IsServer || !(MyAPIGateway.Utilities?.IsDedicated ?? false);
        }

        #endregion
    }
}