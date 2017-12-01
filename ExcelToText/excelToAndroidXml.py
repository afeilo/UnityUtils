#coding='utf-8'
import xlrd
import sys

def export(path):
    data = xlrd.open_workbook(path, encoding_override="utf-8")
    table = data.sheets()[0]
    f = open(path.split('.')[0] + '.xml', 'wb')
    f.write(b'<?xml version="1.0" encoding="utf-8" ?>\n')
    f.write(b'<resources>\n' )
    for i in range(0, table.nrows):
        s = u"\t<%s name = \"%s\">%s</%s>\n" % (str(table.cell_value(i,0)),str(table.cell_value(i,1)),str(table.cell_value(i,2)),str(table.cell_value(i,0)))
        f.write(s.encode())
    f.write(b'</resources>\n' )
def exportExcel():
    for i in range(1, len(sys.argv)):
        export(sys.argv[i])

exportExcel()