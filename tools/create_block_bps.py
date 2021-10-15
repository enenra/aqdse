import os
import xml.etree.ElementTree as ET
import xml.dom.minidom


def main():

    blocks = []
    files = []

    src = os.path.join(os.path.dirname(__file__), 'Data')

    for r, d, f in os.walk(src):
        for file in f:
            if '.sbc' in file:
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
    bpce = ET.SubElement(defs, 'BlueprintClassEntries')

    for b in blocks:
        entry = ET.SubElement(bpce, 'Entry')
        if b[2] == "Large":
            entry.set('Class', "LargeBlocks")
        else:
            entry.set('Class', "SmallBlocks")
        entry.set('BlueprintSubtypeId', b[0] + '/' + b[1])

    temp_string = ET.tostring(defs, 'utf-8')
    temp_string.decode('ascii')
    xml_string = xml.dom.minidom.parseString(temp_string)
    xml_formatted = xml_string.toprettyxml()

    target_file = os.path.join(src, "BlueprintClassEntries.sbc")
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