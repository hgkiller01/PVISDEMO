﻿function downloadxlsx(filename, sheetname, data) {
    //儲存xlsx檔

    //datenum
    function datenum(v, date1904) {
        if (date1904) v += 1462;
        var epoch = Date.parse(v);
        return (epoch - new Date(Date.UTC(1899, 11, 30))) / (24 * 60 * 60 * 1000);
    }

    //sheet_from_array_of_arrays
    function sheet_from_array_of_arrays(data, opts) {
        var ws = {};
        var range = { s: { c: 10000000, r: 10000000 }, e: { c: 0, r: 0 } };
        for (var R = 0; R != data.length; ++R) {
            for (var C = 0; C != data[R].length; ++C) {
                if (range.s.r > R) range.s.r = R;
                if (range.s.c > C) range.s.c = C;
                if (range.e.r < R) range.e.r = R;
                if (range.e.c < C) range.e.c = C;
                var cell = { v: data[R][C] };
                if (cell.v == null) continue;
                var cell_ref = XLSX.utils.encode_cell({ c: C, r: R });

                if (typeof cell.v === 'number') cell.t = 'n';
                else if (typeof cell.v === 'boolean') cell.t = 'b';
                else if (cell.v instanceof Date) {
                    cell.t = 'n'; cell.z = XLSX.SSF._table[14];
                    cell.v = datenum(cell.v);
                }
                else cell.t = 's';

                ws[cell_ref] = cell;
            }
        }
        if (range.s.c < 10000000) ws['!ref'] = XLSX.utils.encode_range(range);
        return ws;
    }

    //s2ab
    function s2ab(s) {
        var buf = new ArrayBuffer(s.length);
        var view = new Uint8Array(buf);
        for (var i = 0; i != s.length; ++i) view[i] = s.charCodeAt(i) & 0xFF;
        return buf;
    }

    //Workbook
    function Workbook() {
        if (!(this instanceof Workbook)) return new Workbook();
        this.SheetNames = [];
        this.Sheets = {};
    }

    //write
    var wb = new Workbook();
    var ws = sheet_from_array_of_arrays(data);
    wb.SheetNames.push(sheetname);
    wb.Sheets[sheetname] = ws;
    var wbout = XLSX.write(wb, { bookType: 'xlsx', type: 'binary' });

    //saveAs
    saveAs(new Blob([s2ab(wbout)], { type: "application/octet-stream" }), filename)

}

function FormatDate(date) {
    var y = date.getFullYear();
    var m = date.getMonth() + 1;
    m = m < 10 ? '0' + m : m;
    var d = date.getDate();
    d = d < 10 ? ('0' + d) : d;
    return y + '/' + m + '/' + d;
}