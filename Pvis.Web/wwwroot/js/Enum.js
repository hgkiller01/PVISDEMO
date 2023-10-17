const Enum = {
    /**
     * 取得Value
     * @param {Object} obj 物件
     * @param {any} key 鍵值
     */
    GetValue: function (obj, key) {
        if (!key || !obj.hasOwnProperty(key)) return '';

        return obj[key];
    },
    /**
     * 再生能源發電設備型別及使用能源
     */
    PowerEquipType: {
        1: '第一型再生能源發電設備－太陽光電發電設備',
        2: '第二型再生能源發電設備－太陽光電發電設備',
        3: '第三型再生能源發電設備－太陽光電發電設備'
    },
}