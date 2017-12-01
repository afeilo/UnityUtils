#coding='utf-8'
import xlrd
import sys

def export(path):
    data = xlrd.open_workbook(path, encoding_override="utf-8")
    table = data.sheets()[0]
    f = open(path.split('.')[0] + '.string', 'wb')
    for i in range(0, table.nrows):
        s = u"\"%s\" = \"%s\";\n" % (str(table.cell_value(i,1)),str(table.cell_value(i,2)))
        f.write(s.encode())
def exportExcel():
    for i in range(1, len(sys.argv)):
        export(sys.argv[i])

exportExcel()