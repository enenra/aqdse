import os
import xml.etree.ElementTree as ET
import xml.dom.minidom


def main():

    blocks = []
    files = []

    src = os.path.join(os.path.dirname(__file__), 'Data')

    for r, d, f in os.walk(src):
        for file in f:
            if '.sbc' in file and "obsolete" not in r:
                files.append(os.path.join(r, file))

    for filename in files:
        with open(os.path.join(src, filename), 'r') as f:
            lines = f.read()
        f.close()

        if lines.find("<CubeBlocks>") == -1:
            continue

        while lines.find("<Definition") != -1:
            blocks.append(get_info(lines))
            lines = lines[lines.find("</Definition>") + len("</Definition>"):]

    defs = ET.Element('Definitions')
    defs.set('xmlns:xsi', 'http://www.w3.org/2001/XMLSchema-instance')
    defs.set('xmlns:xsd', 'http://www.w3.org/2001/XMLSchema')
    rbs = ET.SubElement(defs, 'ResearchBlocks')

    for b in blocks:
        rb = ET.SubElement(rbs, 'ResearchBlock')
        rb.set('xsi:type', "ResearchBlock")
        id = ET.SubElement(rb, 'Id')
        id.set('Type', b[0])
        id.set('Subtype', b[1])
        ubg = ET.SubElement(rb, 'UnlockedByGroups')
        gs = ET.SubElement(ubg, 'GroupSubtype')
        gs.text = "1"

    temp_string = ET.tostring(defs, 'utf-8')
    temp_string.decode('ascii')
    xml_string = xml.dom.minidom.parseString(temp_string)
    xml_formatted = xml_string.toprettyxml()

    target_file = os.path.join(src, "ResearchBlocks.sbc")
    exported_xml = open(target_file, "w")
    exported_xml.write(xml_formatted)

    return


def get_info(lines):
    return [get_tag_content(lines, "TypeId"), get_tag_content(lines, "SubtypeId"), get_tag_content(lines, "CubeSize")]


def get_tag_content(lines, tag):
    content = lines[lines.find('<' + tag + '>') + len('<' + tag + '>'):]
    content = content[:content.find('</' + tag + '>')]
    return content

if __name__ == '__main__':
    main()