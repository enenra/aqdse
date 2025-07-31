import os
import shutil

MOD_PATH = "C:\\Modding\\A Quantum of Depth\\AQD - Concrete\\Content"

GAME_DATA_DIR = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\SpaceEngineers\\Content\\Data"

def main():
    src = os.path.join(GAME_DATA_DIR, 'CubeBlocks')
    dst = os.path.join(MOD_PATH, 'Data', 'CubeBlocks')

    for r, d, f in os.walk(src):
        for file in f:
            if file.startswith("CubeBlocks_Armor") and file.endswith(".sbc") and not file.endswith("Panels.sbc"):
                with open(os.path.join(src, file), 'r') as wf:
                    lines = wf.read()
                    wf.close()

                count = lines.count("<Definition>")
                entries = {}
                entries_hvy = {}

                # Skip SG & Heavy armor
                progress = 0
                while get_subelement(lines, "Definition") != -1:
                    entry = get_subelement(lines, "Definition")
                    subtypeid = get_subelement(entry, "SubtypeId").replace("<SubtypeId>", "").replace("</SubtypeId>", "")

                    progress += 1
                    print("-------------------------------------------------------------------------------------")
                    print(f"{progress} / {count}" )
                    print(subtypeid)

                    public = ""
                    if get_subelement(entry, "Public") != -1:
                        public = get_subelement(entry, "Public").replace("<Public>", "").replace("</Public>", "")

                    if subtypeid.startswith("Small") or get_subelement(lines, "CubeSize") == "<CubeSize>Small</CubeSize>" or public == "false":
                        print("skipping...")
                    elif "Heavy" in subtypeid:
                        print("adding to hvy...")
                        entries_hvy[subtypeid] = entry
                    else:
                        print("adding...")
                        entries[subtypeid] = entry

                    lines = lines[lines.find("</Definition>") + len("</Definition>"):]

                # Change SubtypeIds
                entries, side_transfer = make_cubedef_adjustments(entries, False)
                lines_light = '<?xml version="1.0" encoding="utf-8"?>\n<Definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">\n\t<CubeBlocks>\n\t\t'
                for v in entries.values():
                    lines_light += v
                lines_light += "\n\t</CubeBlocks>\n</Definitions>"
                lines_light = lines_light.replace("    ", "\t")

                target_file = os.path.join(dst, file.replace("CubeBlocks_Armor", "AQD_Concrete"))
                exported_xml = open(target_file, "w")
                exported_xml.write(lines_light)

                entries_hvy, unused = make_cubedef_adjustments(entries_hvy, True, side_transfer)
                lines_hvy = '<?xml version="1.0" encoding="utf-8"?>\n<Definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">\n\t<CubeBlocks>\n\t\t'
                for v in entries_hvy.values():
                    lines_hvy += v
                lines_hvy += "\n\t</CubeBlocks>\n</Definitions>"
                lines_hvy = lines_hvy.replace("    ", "\t")

                target_file = os.path.join(dst, file.replace("CubeBlocks_Armor", "AQD_ReinforcedConcrete"))
                exported_xml = open(target_file, "w")
                exported_xml.write(lines_hvy)

    return


def make_cubedef_adjustments(entries, hvy, side_transfer: dict = None):

    if hvy:
        id = "ReinfConc"
    else:
        id = "Conc"
        side_transfer = {}

    adjusted_subtypes = {}
    for k, v in entries.items():
        if k.startswith("LargeHeavyBlockArmor"):
            k_n = k.replace("LargeHeavyBlockArmor", f"AQD_LG_{id}_")
        elif k.startswith("LargeBlockHeavyArmor"):
            k_n = k.replace("LargeBlockHeavyArmor", f"AQD_LG_{id}_")
        elif k.startswith("LargeBlockArmor"):
            k_n = k.replace("LargeBlockArmor", f"AQD_LG_{id}_")
        elif k.startswith("LargeHeavy"):
            k_n = k.replace("LargeHeavy", f"AQD_LG_{id}_")
        else:
            k_n = k.replace("Large", f"AQD_LG_{id}_")

        v_n = v.replace(k, k_n)

        # Change DisplayName + Description
        dpname = get_subelement(v_n, "DisplayName")
        v_n = v_n.replace(dpname, f"<DisplayName>DisplayName_{k_n}</DisplayName>")
        desc = get_subelement(v_n, "Description")
        v_n = v_n.replace(desc, f"<Description>Description_AQD_{id}_Block</Description>")

        # Change Icon
        icon = get_subelement(v_n, "Icon")
        v_n = v_n.replace(icon, f"<Icon>Textures\\GUI\\Icons\\Cubes\\{k_n}.dds</Icon>")

        if hvy:
            icon = get_subelement(v_n, "Icon")
            v_n = v_n.replace(icon, icon.replace("_ReinfConc_", "_Conc_").replace("</Icon>", f"</Icon>\n\t\t\t<Icon>Textures\\GUI\\Icons\\Cubes\\AQD_ReinforcedConcrete.dds</Icon>"))

        # Change ShowEdges
        showedges = get_subelement(v_n, "ShowEdges")
        v_n = v_n.replace(showedges, f"<ShowEdges>false</ShowEdges>")

        # Change CubeDefinition paths
        sides = get_subelement(v_n, "Sides")

        if hvy:
            v_n = v_n.replace(sides, side_transfer[k_n.replace("_ReinfConc_", "_Conc_")].replace("C_", "RC_"))

        else:
            sides_copy = sides
            sides_n = "<Sides>\n"

            side_id = "C"
            if id == "ReinfConc":
                side_id = "RC"

            while get_subelement(sides, "Side") != -1:
                side = sides[sides.find("<Side "):sides.find("/>")+2]
                plate = side[side.find("\\Armor\\") + len("\\Armor\\"):side.find(".mwm")]
                sides_n += "\t\t\t\t\t" + side.replace(plate, f"{side_id}_{plate.replace("Heavy", "").replace("LightArmor", "").replace("CornerTriangle", "CorTri")}") + "\n"
                sides = sides[sides.find("/>") + len("/>"):]
            sides_n += "\t\t\t\t</Sides>"
            side_transfer[k_n] = sides_n
            v_n = v_n.replace(sides_copy, sides_n)

        # Change component cost
        comps = get_subelement(v_n, "Components")
        comps_n = comps.replace("SteelPlate", "AQD_Comp_Concrete")
        v_n = v_n.replace(comps, comps_n)

        v_n = v_n.replace('<CriticalComponent Subtype="SteelPlate" Index="0" />', '<CriticalComponent Subtype="AQD_Comp_Concrete" Index="0" />')

        # Change BlockPairName
        bpn = get_subelement(v_n, "BlockPairName")
        v_n = v_n.replace(bpn, f"<BlockPairName>{k_n.replace("_LG_", "_")}</BlockPairName>")

        # Change MirroringBlock
        mb = get_subelement(v_n, "MirroringBlock")
        if mb != -1:
            mb_id = mb.split(">")[1].split("<")[0]

            if mb_id.startswith("LargeHeavyBlockArmor"):
                mb_id = mb_id.replace("LargeHeavyBlockArmor", f"AQD_LG_{id}_")
            elif mb_id.startswith("LargeBlockHeavyArmor"):
                mb_id = mb_id.replace("LargeBlockHeavyArmor", f"AQD_LG_{id}_")
            elif mb_id.startswith("LargeBlockArmor"):
                mb_id = mb_id.replace("LargeBlockArmor", f"AQD_LG_{id}_")
            else:
                mb_id = mb_id.replace("Large", f"AQD_LG_{id}_")

            v_n = v_n.replace(mb, f"<MirroringBlock>{mb_id}</MirroringBlock>")

        # Add PhysicalMaterial & UsesDeformation
        v_n = v_n.replace("</PCUConsole>", "</PCUConsole>\n\t\t\t<PhysicalMaterial>Rock</PhysicalMaterial>\n\t\t\t<DLC>ScrapRace</DLC>")
        v_n = v_n.replace("</PhysicalMaterial>", "</PhysicalMaterial>\n\t\t\t<UsesDeformation>false</UsesDeformation>")

        v_n += "\n\t\t"
        adjusted_subtypes[k_n] = v_n

    return adjusted_subtypes, side_transfer


def get_subelement(lines: str, name: str, attrib: str = None):
    """Returns the specified subelement. -1 if not found."""

    if attrib is not None and f"<{name} name=\"{attrib}\">" in lines:
        start = lines.find(f"<{name} name=\"{attrib}\">")
        end = start + lines[start:].find(f"</{name}>") + len(f"</{name}>")
        return lines[start:end]

    elif f"<{name}>" in lines:
        start = lines.find(f"<{name}>")
        end = start + lines[start:].find(f"</{name}>") + len(f"</{name}>")
        return lines[start:end]

    elif f"<{name} " in lines:
        start = lines.find(f"<{name} ")
        end = start + lines[start:].find('/>') + 2
        return lines[start:end]

    else:
        return -1

if __name__ == '__main__':
    main()