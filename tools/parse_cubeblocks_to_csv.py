import os
import xml.etree.ElementTree as ET
import xml.dom.minidom


def main():

    blocks = []
    files = []
    output = ""

    src = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\SpaceEngineers\\Content\\Data\\CubeBlocks\\"
    resx = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\SpaceEngineers\\Content\\Data\\Localization\\MyTexts.resx"

    for r, d, f in os.walk(src):
        for file in f:
            if '.sbc' in file:
                files.append(os.path.join(r, file))

    for filename in files:
        with open(os.path.join(src, filename), 'r') as f:
            lines = f.read()
        f.close()

        if not "<CubeBlocks>" in lines:
            continue

        while "<Definition" in lines:
            lines = lines[lines.find("<Definition"):]
            entry = lines[:lines.find("</Definition>") + len("</Definition>")]
            lines = lines[lines.find("</Definition>") + len("</Definition>"):]
            blocks.append(get_info(entry))
    
    with open(resx, 'r', encoding="utf-8") as f:
        resx_lines = f.read()

    for b in blocks:
        if b[4] is not None and b[4] == 'false':
            continue
        name = b[3]
        if f"<data name=\"{name}\" xml:space=\"preserve\">" in resx_lines:
            entry = resx_lines[resx_lines.find(f"<data name=\"{name}\" xml:space=\"preserve\">") + len((f"<data name=\"{name}\" xml:space=\"preserve\">")):]
            entry = entry[:entry.find("</data>")]
            name = entry[entry.find("<value>") + len("<value>"):]
            name = name[:name.find("</value>")]
        output += f"({b[2]}) {name},{b[0]},{b[1]}\n"

    target_file = os.path.join(os.path.dirname(__file__), "output.csv")
    exported_xml = open(target_file, "w")
    exported_xml.write(output)

    return


def get_info(lines):
    typeid = get_tag_content(lines, "TypeId")
    if not typeid.startswith("MyObjectBuilder_"):
        typeid = "MyObjectBuilder_" + typeid
    return [typeid, get_tag_content(lines, "SubtypeId"), get_tag_content(lines, "CubeSize"), get_tag_content(lines, "DisplayName"), get_tag_content(lines, "Public")]


def get_tag_content(lines, tag):

    if f"<{tag} />" in lines or f"<{tag}/>" in lines:
        return "(null)"
    elif not tag in lines:
        return None
    else:
        content = lines[lines.find(f"<{tag}>") + len(f"<{tag}>"):]
        content = content[:content.find(f"</{tag}>")]
        return content

if __name__ == '__main__':
    main()