import os
import xml.etree.ElementTree as ET
import xml.dom.minidom

MOD_PATH = "C:\\Modding\\A Quantum of Depth\\AQD - Concrete\\Content"

GAME_DATA_DIR = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\SpaceEngineers\\Content\\Data"

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


def create_bvgs():
    blocks = []
    src = os.path.join(MOD_PATH, "Data", "CubeBlocks")
    for r, d, f in os.walk(src):
        for file in f:
            if file.endswith(".sbc") and "obsolete" not in r:
                with open(os.path.join(src, file), 'r') as wf:
                    lines = wf.read()
                    while get_subelement(lines, "Definition") != -1:
                        subtype = get_subelement(get_subelement(lines, "Definition"), "SubtypeId").replace("<SubtypeId>", "").replace("</SubtypeId>", "")
                        if subtype not in blocks:
                            blocks.append(subtype)
                        lines = lines[lines.find("</Definition>") + len("</Definition>"):]
                    wf.close()

    assignments = {}
    src = os.path.join(GAME_DATA_DIR)
    for r, d, f in os.walk(src):
        for file in f:
            if file != "BlockVariantGroups.sbc":
                continue
            with open(os.path.join(src, file), 'r') as wf:
                lines = wf.read()
                while get_subelement(lines, "BlockVariantGroup") != -1:
                    entry = get_subelement(lines, "BlockVariantGroup")
                    subtype = get_subelement(entry, "Id").split('"')[3]

                    if subtype.startswith("ArmorLight"):
                        new_subtype = subtype.replace("ArmorLight", "AQD_BVG_Concrete_")
                        if not new_subtype in assignments.keys():
                            assignments[new_subtype] = []

                        while get_subelement(entry, "Block") != -1:
                            entry_block = get_subelement(entry, "Block")
                            entry_block = entry_block.split('"')[3]

                            if not entry_block.startswith("Small"):
                                id = "Conc"
                                if entry_block.startswith("LargeHeavyBlockArmor"):
                                    entry_block = entry_block.replace("LargeHeavyBlockArmor", f"AQD_LG_{id}_")
                                elif entry_block.startswith("LargeBlockHeavyArmor"):
                                    entry_block = entry_block.replace("LargeBlockHeavyArmor", f"AQD_LG_{id}_")
                                elif entry_block.startswith("LargeBlockArmor"):
                                    entry_block = entry_block.replace("LargeBlockArmor", f"AQD_LG_{id}_")
                                else:
                                    entry_block = entry_block.replace("Large", f"AQD_LG_{id}_")

                                if f"AQD_LG_{id}_" not in entry_block:
                                    entry_block = f"AQD_LG_{id}_" + entry_block

                                if entry_block.replace("LargeBlock", "AQD_Conc_") not in assignments[new_subtype]:
                                    assignments[new_subtype].append(entry_block.replace("LargeBlock", "AQD_Conc_"))

                            entry = entry[entry.find(" />") + len(" />"):]

                    lines = lines[lines.find("</BlockVariantGroup>") + len("</BlockVariantGroup>"):]
                wf.close()

    defs = ET.Element('Definitions')
    defs.set('xmlns:xsi', 'http://www.w3.org/2001/XMLSchema-instance')
    defs.set('xmlns:xsd', 'http://www.w3.org/2001/XMLSchema')
    bvgs = ET.SubElement(defs, 'BlockVariantGroups')

    for bvg_subtype, entry_blocks in assignments.items():
        bvg = ET.SubElement(bvgs, 'BlockVariantGroup')

        id = ET.SubElement(bvg, 'Id')
        id.set("Type", "MyObjectBuilder_BlockVariantGroup")
        id.set("Subtype", bvg_subtype)

        icon = ET.SubElement(bvg, 'Icon')
        icon.text = f"Textures\\GUI\\Icons\\Cubes\\{entry_blocks[0]}.dds"

        dpname = ET.SubElement(bvg, 'DisplayName')
        dpname.text = f"DisplayName_{entry_blocks[0]}"

        descr = ET.SubElement(bvg, 'Description')
        descr.text = "Description_AQD_Concrete_Block"

        bvg_entries = ET.SubElement(bvg, 'Blocks')

        for b in entry_blocks:
            entry = ET.SubElement(bvg_entries, 'Block')
            entry.set("Type", "MyObjectBuilder_CubeBlock")
            entry.set("Subtype", b)

    temp_string = ET.tostring(defs, 'utf-8')
    temp_string.decode('ascii')
    xml_string = xml.dom.minidom.parseString(temp_string)
    xml_formatted = xml_string.toprettyxml()

    # Concrete
    target_file = os.path.join(MOD_PATH, 'Data', "BlockVariantGroups_Concrete.sbc")
    exported_xml = open(target_file, "w")
    exported_xml.write(xml_formatted)

    # ReinforcedConcrete
    target_file = os.path.join(MOD_PATH, 'Data', "BlockVariantGroups_ReinforcedConcrete.sbc")
    exported_xml = open(target_file, "w")
    xml_formatted = xml_formatted.replace("_Conc_", "_ReinfConc_").replace("_BVG_Concrete_", "_BVG_ReinforcedConcrete_")

    icons = []
    lines = xml_formatted
    while get_subelement(lines, "Icon") != -1:
        icons.append(get_subelement(lines, "Icon").split('>')[1].split('<')[0])
        lines = lines[lines.find("</Icon>") + len("</Icon>"):]

    for i in icons:
        xml_formatted = xml_formatted.replace(i, i.replace("_ReinfConc_", "_Conc_"))

    exported_xml.write(xml_formatted)

create_bvgs()