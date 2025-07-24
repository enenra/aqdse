import bpy
import os
import shutil

GAME_DATA_DIR = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\SpaceEngineers\\Content\\Data"
SDK_DIR = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\SpaceEngineersModSDK\\OriginalContent"

DESTINATION = "C:\\Modding\\A Quantum of Depth\\AQD - Concrete\\OriginalContent"

src = os.path.join(GAME_DATA_DIR, 'CubeBlocks')


def remove_between(text, start, end):
    start_idx = text.find(start)
    end_idx = text.find(end, start_idx + len(start))

    if start_idx != -1 and end_idx != -1:
        return text[:start_idx] + text[end_idx + len(end):]
    return text


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


def find_plates():
    plates = []
    for r, d, f in os.walk(src):
        for file in f:
            if file.startswith("CubeBlocks_Armor") and file.endswith(".sbc") and not file.endswith("Panels.sbc"):
                with open(os.path.join(src, file), 'r') as wf:
                    lines = wf.read()
                    wf.close()

                    while get_subelement(lines, "Definition") != -1:
                        entry = get_subelement(lines, "Definition")
                        subtypeid = get_subelement(entry, "SubtypeId").replace("<SubtypeId>", "").replace("</SubtypeId>", "")

                        if subtypeid.startswith("Small") or get_subelement(lines, "CubeSize") == "<CubeSize>Small</CubeSize>" or get_subelement(lines, "EdgeType") == "<EdgeType>Heavy</EdgeType>":
                            lines = lines[lines.find("</Definition>") + len("</Definition>"):]
                            continue

                        sides = get_subelement(lines, "Sides")
                        while "<Side " in sides:
                            side = sides[sides.find("<Side "):sides.find("/>")+2]
                            plate = side[side.find('Model="') + len('Model="'):side.find(".mwm")]

                            if not plate in plates:
                                plates.append(plate)

                            sides = sides[sides.find("/>") + len("/>"):]

                        lines = lines[lines.find("</Definition>") + len("</Definition>"):]

        return plates


def find_plate_xmls(plates):
    xmls = []

    for p in plates:
        xmls.append(os.path.join(SDK_DIR, p + ".xml"))

    return xmls


def copy_adjust_xmls(xmls):
    for xml in xmls:
        with open(xml, 'r') as f:
            lines = f.read()
            f.close()

        # TODO: Adjust this to merge with xmls exported from SEUT

        lines = lines.replace("\n</Model>", '\n\t<Material Name="Concrete_Armor">\n\t\t<Parameter Name="Technique">MESH</Parameter>\n\t\t<Parameter Name="ColorMetalTexture">Textures\\Models\\Cubes\\armor\\Skins\\Concrete\\large_square_plate_cm.dds</Parameter>\n\t\t<Parameter Name="NormalGlossTexture">Textures\\Models\\Cubes\\armor\\Skins\Concrete\\large_square_plate_ng.dds</Parameter>\n\t\t<Parameter Name="AddMapsTexture">Textures\Models\\Cubes\\armor\\Skins\\Concrete\\large_square_plate_add.dds</Parameter>\n\t</Material>\n</Model>')

        while "<LOD " in lines:
            lines = remove_between(lines, "<LOD ", "</LOD>\n")

        filename = os.path.basename(xml)
        filename = "AQD_Concrete_" + filename.replace("LightArmor", "")

        dst = os.path.join(DESTINATION, 'Models', 'Cubes', 'large', 'armor')
        target_file = os.path.join(dst, filename)
        exported_xml = open(target_file, "w")
        exported_xml.write(lines)

        # Reinforced versions
        lines = lines.replace("AQD_Concrete_", "AQD_ReinforcedConcrete_")
        lines = lines.replace("Concrete_Armor", "ReinforcedConcrete_Armor")

        target_file = os.path.join(dst, filename.replace("AQD_Concrete_", "AQD_ReinforcedConcrete_"))
        exported_xml = open(target_file, "w")
        exported_xml.write(lines)



plates = find_plates()

xmls = find_plate_xmls(plates)
copy_adjust_xmls(xmls)