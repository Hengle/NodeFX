node = hou.pwd()
definition = node.type().definition()
path = node.parm("HDApath").get()

definition.save(path, node)