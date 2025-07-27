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
            if file.endswith(".sbc"):
                with open(os.path.join(src, file), 'r') as wf:
                    lines = wf.read()
                    while get_subelement(lines, "Definition") != -1:
                        blocks.append(get_subelement(get_subelement(lines, "Definition"), "SubtypeId").replace("<SubtypeId>", "").replace("</SubtypeId>", ""))
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
                                id = "Concrete"
                                if entry_block.startswith("LargeHeavyBlockArmor"):
                                    entry_block = entry_block.replace("LargeHeavyBlockArmor", f"AQD_LG_{id}_")
                                elif entry_block.startswith("LargeBlockHeavyArmor"):
                                    entry_block = entry_block.replace("LargeBlockHeavyArmor", f"AQD_LG_{id}_")
                                elif entry_block.startswith("LargeBlockArmor"):
                                    entry_block = entry_block.replace("LargeBlockArmor", f"AQD_LG_{id}_")
                                else:
                                    entry_block = entry_block.replace("Large", f"AQD_LG_{id}_")

                                assignments[new_subtype].append(entry_block.replace("LargeBlock", "AQD_Concrete_"))

                            entry = entry[entry.find(" />") + len(" />"):]

                    lines = lines[lines.find("</BlockVariantGroup>") + len("</BlockVariantGroup>"):]
                wf.close()

    print(assignments)
    return

    defs = ET.Element('Definitions')
    defs.set('xmlns:xsi', 'http://www.w3.org/2001/XMLSchema-instance')
    defs.set('xmlns:xsd', 'http://www.w3.org/2001/XMLSchema')
    bvgs = ET.SubElement(defs, 'BlockVariantGroups')

    bvg_letters = ET.SubElement(bvgs, 'BlockVariantGroup')
    bvg_numbers = ET.SubElement(bvgs, 'BlockVariantGroup')
    bvg_symbols = ET.SubElement(bvgs, 'BlockVariantGroup')

    hdr_letters = False
    hdr_numbers = False
    hdr_symbols = False
    for b in blocks:
        if b.endswith(tuple(numbers)):
            if not hdr_numbers:
                id = ET.SubElement(bvg_numbers, 'Id')
                id.set("Type", "MyObjectBuilder_BlockVariantGroup")
                id.set("Subtype", f"{prefix}Numbers1")

                icon = ET.SubElement(bvg_numbers, 'Icon')
                icon.text = f"Textures\GUI\Icons\Cubes\{prefix}LG_Symbol_0.dds"

                dpname = ET.SubElement(bvg_numbers, 'DisplayName')
                dpname.text = f"DisplayName_{prefix}LG_Symbol_0"

                descr = ET.SubElement(bvg_numbers, 'Description')
                descr.text = "Description_Numbers"

                n_entries = ET.SubElement(bvg_numbers, 'Blocks')

                hdr_numbers = True

            entry = ET.SubElement(n_entries, 'Block')
            entry.set("Type", "MyObjectBuilder_CubeBlock")
            entry.set("Subtype", b)

        elif b.endswith(tuple(symbols)):
            if not hdr_symbols:
                id = ET.SubElement(bvg_symbols, 'Id')
                id.set("Type", "MyObjectBuilder_BlockVariantGroup")
                id.set("Subtype", f"{prefix}Symbols")

                icon = ET.SubElement(bvg_symbols, 'Icon')
                icon.text = f"Textures\GUI\Icons\Cubes\{prefix}LG_Symbol_And.dds"

                dpname = ET.SubElement(bvg_symbols, 'DisplayName')
                dpname.text = f"DisplayName_{prefix}LG_Symbol_And"

                descr = ET.SubElement(bvg_symbols, 'Description')
                descr.text = "Description_Symbols"

                s_entries = ET.SubElement(bvg_symbols, 'Blocks')

                hdr_symbols = True

            entry = ET.SubElement(s_entries, 'Block')
            entry.set("Type", "MyObjectBuilder_CubeBlock")
            entry.set("Subtype", b)

        else:
            if not hdr_letters:
                id = ET.SubElement(bvg_letters, 'Id')
                id.set("Type", "MyObjectBuilder_BlockVariantGroup")
                id.set("Subtype", f"{prefix}LettersAZ")

                icon = ET.SubElement(bvg_letters, 'Icon')
                icon.text = f"Textures\GUI\Icons\Cubes\{prefix}LG_Symbol_A.dds"

                dpname = ET.SubElement(bvg_letters, 'DisplayName')
                dpname.text = f"DisplayName_{prefix}LG_Symbol_A"

                descr = ET.SubElement(bvg_letters, 'Description')
                descr.text = "Description_Letters"

                l_entries = ET.SubElement(bvg_letters, 'Blocks')

                hdr_letters = True

            entry = ET.SubElement(l_entries, 'Block')
            entry.set("Type", "MyObjectBuilder_CubeBlock")
            entry.set("Subtype", b)


    temp_string = ET.tostring(defs, 'utf-8')
    temp_string.decode('ascii')
    xml_string = xml.dom.minidom.parseString(temp_string)
    xml_formatted = xml_string.toprettyxml()

    target_file = os.path.join(mod_path, 'Data', "BlockVariantGroups.sbc")
    exported_xml = open(target_file, "w")
    exported_xml.write(xml_formatted)

create_bvgs()