import xml.etree.cElementTree as ET
import hou

geo = hou.pwd().geometry()
numEmitters = hou.pwd().geometry().intAttribValue("numEmitters")
root = ET.Element("root")
root.set("emitterCount", str(numEmitters))

#   Loop over emitters
for i in range(0, numEmitters):
    emitterName = "emitter" + str(i)
    emitterElement = ET.SubElement(root, emitterName)

    #   Loop over attributes in emitter
    for attribute in geo.pointAttribs():
        attributeElement = ET.SubElement(emitterElement, "attribute")
        attributeElement.text = attribute.name()

        if attribute.dataType() == hou.attribData.Int:
            attributeElement.set("type", "int")
            values = geo.iterPoints()[i].intListAttribValue(attribute.name())

        if attribute.dataType() == hou.attribData.Float:
            attributeElement.set("type", "float")
            values = geo.iterPoints()[i].floatListAttribValue(attribute.name())

        if attribute.dataType() == hou.attribData.String:
            attributeElement.set("type", "string")
            values = geo.iterPoints()[i].stringListAttribValue(attribute.name())

        #   Loop over values in attribute
        for index, value in enumerate(values):
            valueElement = ET.SubElement(attributeElement, "value")
            valueElement.set("index", str(index))
            valueElement.text = str(value)

ET.ElementTree(root).write("filename.xml")
