import os
import xml.etree.ElementTree as ET
import xml.dom.minidom

MOD_PATH = "C:\\Modding\\A Quantum of Depth\\AQD - Concrete\\Content"

GAME_DATA_DIR = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\SpaceEngineers\\Content\\Data"

def get_subelement(lines: str, name: str, attrib: str = None):
    """Returns the specified subelement. -1 if not found."""

    if attrib is not None and f"<{name} name=\"{attrib}\" " in lines:
        start = lines.find(f"<{name} name=\"{attrib}\" ")
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


def create_resx_entry(defs, subtypeid, name):
    data = ET.SubElement(defs, 'data')
    data.set("name", f"DisplayName_{subtypeid}")
    data.set("xml:space", "preserve")

    value = ET.SubElement(data, 'value')
    value.text = name

    comment = ET.SubElement(data, 'comment')


def generate_resx():
    resx_entries = {}

    with open(os.path.join(GAME_DATA_DIR, "Localization", "MyTexts.resx"), 'r') as wf:
        game_resx = wf.read()
        wf.close()

    src = os.path.join(GAME_DATA_DIR, "CubeBlocks")
    for r, d, f in os.walk(src):
        for file in f:
            if file.startswith("CubeBlocks_Armor") and file.endswith(".sbc") and not file.endswith("Panels.sbc"):
                with open(os.path.join(src, file), 'r') as wf:
                    lines = wf.read()
                    wf.close()

                while get_subelement(lines, "Definition") != -1:
                    entry = get_subelement(lines, "Definition")
                    subtype = get_subelement(get_subelement(lines, "Definition"), "SubtypeId").replace("<SubtypeId>", "").replace("</SubtypeId>", "")

                    public = ""
                    if get_subelement(entry, "Public") != -1:
                        public = get_subelement(entry, "Public").replace("<Public>", "").replace("</Public>", "")

                    if subtype.startswith("Small") or get_subelement(lines, "CubeSize") == "<CubeSize>Small</CubeSize>" or "Heavy" in subtype or public == "false":
                        lines = lines[lines.find("</Definition>") + len("</Definition>"):]
                        continue

                    dpname = get_subelement(get_subelement(lines, "Definition"), "DisplayName").replace("<DisplayName>", "").replace("</DisplayName>", "")
                    dpname_text = get_subelement(game_resx, "data", dpname).split("<value>")[1].split("</value>")[0]

                    id = "Conc"
                    if subtype.startswith("LargeBlockArmor"):
                        subtype = subtype.replace("LargeBlockArmor", f"AQD_LG_{id}_")
                    else:
                        subtype = subtype.replace("Large", f"AQD_LG_{id}_")

                    if f"AQD_LG_{id}_" not in subtype:
                        subtype = f"AQD_LG_{id}_" + subtype

                    resx_entries[subtype] = dpname_text.replace("Light Armor", "Concrete").replace("Armor", "Concrete")

                    lines = lines[lines.find("</Definition>") + len("</Definition>"):]

        break

    defs = ET.Element('root')

    for k, v in resx_entries.items():
        create_resx_entry(defs, k, v)
        create_resx_entry(defs, k.replace("_Conc_", "_ReinfConc_"), v.replace("Concrete", "Reinf. Concrete"))

    temp_string = ET.tostring(defs, 'utf-8')
    temp_string.decode('ascii')
    xml_string = xml.dom.minidom.parseString(temp_string)
    xml_formatted = xml_string.toprettyxml()

    print(xml_formatted)

    return

    resx_file = os.path.join(MOD_PATH, "Data", "Localization", "MyTexts.resx")
    with open(resx_file, 'r') as f:
        lines_resx = f.read()
        f.close()

    lines_resx

    exported_xml = open(resx_file, "w")
    exported_xml.write(lines_resx)


generate_resx()