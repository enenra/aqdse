import os
import shutil

GAME_DATA_DIR = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\SpaceEngineers\\Content\\Data"

def main():
    src = os.path.join(GAME_DATA_DIR, 'CubeBlocks')
    dst = os.path.join(os.path.dirname(__file__), 'Data')

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

                    if subtypeid.startswith("Small") or get_subelement(lines, "CubeSize") == "<CubeSize>Small</CubeSize>":
                        print("skipping...")
                    elif "Heavy" in subtypeid:
                        print("adding to hvy...")
                        entries_hvy[subtypeid] = entry
                    else:
                        print("adding...")
                        entries[subtypeid] = entry

                    lines = lines[lines.find("</Definition>") + len("</Definition>"):]

                # Change SubtypeIds
                entries = make_cubedef_adjustments(entries, False)
                lines_light = '<?xml version="1.0" encoding="utf-8"?>\n<Definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">\n\t<CubeBlocks>\n\t\t'
                for v in entries.values():
                    lines_light += v
                lines_light += "\n\t</CubeBlocks>\n</Definitions>"
                lines_light = lines_light.replace("    ", "\t")

                target_file = os.path.join(dst, file.replace("CubeBlocks_Armor", "AQD_Concrete"))
                exported_xml = open(target_file, "w")
                exported_xml.write(lines_light)

                entries_hvy = make_cubedef_adjustments(entries_hvy, True)
                lines_hvy = '<?xml version="1.0" encoding="utf-8"?>\n<Definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">\n\t<CubeBlocks>\n\t\t'
                for v in entries_hvy.values():
                    lines_hvy += v
                lines_hvy += "\n\t</CubeBlocks>\n</Definitions>"
                lines_hvy = lines_hvy.replace("    ", "\t")

                target_file = os.path.join(dst, file.replace("CubeBlocks_Armor", "AQD_ReinforcedConcrete"))
                exported_xml = open(target_file, "w")
                exported_xml.write(lines_hvy)

    return


def make_cubedef_adjustments(entries, hvy):

    id = "Concrete"
    if hvy:
        id = "ReinforcedConcrete"

    adjusted_subtypes = {}
    for k, v in entries.items():
        if k.startswith("LargeHeavyBlockArmor"):
            k_n = k.replace("LargeHeavyBlockArmor", f"AQD_LG_{id}_")
        elif k.startswith("LargeBlockHeavyArmor"):
            k_n = k.replace("LargeBlockHeavyArmor", f"AQD_LG_{id}_")
        elif k.startswith("LargeBlockArmor"):
            k_n = k.replace("LargeBlockArmor", f"AQD_LG_{id}_")
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

        # Change CubeDefinition paths
        sides = get_subelement(v_n, "Sides")

        sides_copy = sides
        sides_n = "<Sides>\n"
        while get_subelement(sides, "Side") != -1:
            side = sides[sides.find("<Side "):sides.find("/>")+2]
            plate = side[side.find("\\Armor\\") + len("\\Armor\\"):side.find(".mwm")]
            sides_n += "\t\t\t\t\t" + side.replace(plate, f"AQD_{id}_{plate.replace("Heavy", "")}") + "\n"
            sides = sides[sides.find("/>") + len("/>"):]
        sides_n += "\t\t\t</Sides>"
        v_n = v_n.replace(sides_copy, sides_n)

        # Change component cost
        comps = get_subelement(v_n, "Components")
        comps_n = comps.replace("SteelPlate", "AQD_Comp_Concrete")
        v_n = v_n.replace(comps, comps_n)

        v_n = v_n.replace('<CriticalComponent Subtype="SteelPlate" Index="0" />', '<CriticalComponent Subtype="AQD_Comp_Concrete" Index="0" />')

        # Change BlockPairName
        bpn = get_subelement(v_n, "BlockPairName")
        v_n = v_n.replace(bpn, f"<BlockPairName>{k_n.replace("_LG_", "_")}</BlockPairName>")

        # Add PhysicalMaterial & DeformationRatio
        v_n = v_n.replace("</PCUConsole>", "</PCUConsole>\n\t\t\t<PhysicalMaterial>Rock</PhysicalMaterial>")
        if get_subelement(v_n, "DeformationRatio") == -1:
            v_n = v_n.replace("</PhysicalMaterial>", "</PhysicalMaterial>\n\t\t\t<DeformationRatio>0.0</DeformationRatio>")
        else:
            defrat = get_subelement(v_n, "DeformationRatio")
            v_n = v_n.replace(defrat, f"<DeformationRatio>0.0</DeformationRatio>")

        v_n += "\n\t\t"
        adjusted_subtypes[k_n] = v_n

    return adjusted_subtypes


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