import os
import xml.etree.ElementTree as ET
import xml.dom.minidom

MOD_PATH = "C:\\Modding\\A Quantum of Depth\\AQD - Concrete\\Content"


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


def create_bc_hdr(bcs, aspects):
    bc = ET.SubElement(bcs, 'Category')
    bc.set('xsi:type', 'MyObjectBuilder_GuiBlockCategoryDefinition')

    id = ET.SubElement(bc, 'Id')
    type_id = ET.SubElement(id, 'TypeId')
    type_id.text = "GuiBlockCategoryDefinition"
    subtype_id = ET.SubElement(id, 'SubtypeId')
    if aspects[0] is not None:
        subtype_id.text = aspects[0]

    dpname = ET.SubElement(bc, 'Displayname')
    dpname.text = aspects[1]

    name = ET.SubElement(bc, 'Name')
    name.text = aspects[2]

    search = ET.SubElement(bc, 'StrictSearch')
    search.text = "true"

    entries = ET.SubElement(bc, 'ItemIds')

    return entries


def create_bcs():
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

    defs = ET.Element('Definitions')
    defs.set('xmlns:xsi', 'http://www.w3.org/2001/XMLSchema-instance')
    defs.set('xmlns:xsd', 'http://www.w3.org/2001/XMLSchema')
    bcs = ET.SubElement(defs, 'CategoryClasses')

    lg_entries = create_bc_hdr(bcs, [None, "DisplayName_Category_LargeBlocks", "Section1_Position1_LargeBlocks"])
    nonarmor_entries = create_bc_hdr(bcs, ["Nonarmor", "DisplayName_Category_Nonarmor", "Section1_Position2_Nonarmor"])
    conc_entries = create_bc_hdr(bcs, ["Concrete", "DisplayName_Category_AQD_Cat_Concrete", "Section1_Position3_Concrete"])

    for b in blocks:
        entry = ET.SubElement(lg_entries, 'string')
        entry.text = f"CubeBlock/{b}"

        entry = ET.SubElement(nonarmor_entries, 'string')
        entry.text = f"CubeBlock/{b}"

        entry = ET.SubElement(conc_entries, 'string')
        entry.text = f"CubeBlock/{b}"

    temp_string = ET.tostring(defs, 'utf-8')
    temp_string.decode('ascii')
    xml_string = xml.dom.minidom.parseString(temp_string)
    xml_formatted = xml_string.toprettyxml()

    target_file = os.path.join(MOD_PATH, 'Data', "BlockCategories.sbc")
    exported_xml = open(target_file, "w")
    exported_xml.write(xml_formatted)

create_bcs()