
var UiHelper = {
    /**
     * 取得 QueryString 中參數
     * @param {any} parameterName 參數名稱
     * @returns {any} 取得的參數值
     */
    GetUrlParameter: function (parameterName) {
        var result = null,
            tmp = [];
        var items = location.search.substr(1).split("&");
        for (var index = 0; index < items.length; index++) {
            tmp = items[index].split("=");
            if (tmp[0] === parameterName) result = decodeURIComponent(tmp[1]);
        }
        return result;
    },
    /**
     * 檔案上傳前端JS判讀
     * @param {any} file 上傳物件
     * @param {any} callfn 檔案取完成 call back 函數
     */
    GetFile: function (file, callfn) {
        var reader = new FileReader();
        reader.onload = function (e) {
            var f = {
                name: file.name,
                size: file.size,
                body: e.target.result.split(',')[1],
                mimetype: e.target.result.split(';')[0].split(':')[1]
            };
            if (callfn) callfn(f);
        };
        reader.readAsDataURL(file);
    },
    /**
     * 遮罩目前操作畫面
     * @param {any} _msg 自訂遮罩時使用文字
     */
    blockUI: function (_msg) {
        $.blockUI({
            baseZ: 2000,
            css: {
                'font-size': '150%',
                border: 'none',
                padding: '15px',
                backgroundColor: '#000',
                '-webkit-border-radius': '10px',
                '-moz-border-radius': '10px',
                'border-radius': '10px',
                opacity: .8,
                color: '#fff'
            },
            message: _msg || "資料處理中，請稍後...<div class='ajax-loader'><div>"
        });
    },
    /** 關閉畫面遮罩 */
    unblockUI: function () {
        $.unblockUI();
    },
    _NotifyCallback: function (isClicked) {
        if (isClicked) alertify.dismissAll();
    },
    /** 清空全部浮動訊息 
     * @returns {any} UiHelper
     */
    ClearMsg: function () {
        alertify.dismissAll();
        return this;
    },
    /**
     * 產生錯誤訊息提示
     * @param {any} Mesages 訊息內容
     * @param {any} wait 訊息停留秒數
     */
    ShowErr: function (Mesages, wait) {
        var me = this;
        if (typeof wait === "undefined") wait = 10;
        if (typeof Mesages === "string") {
            alertify.notify(Mesages, "error", wait, me._NotifyCallback);
            return;
        }
        if (!Mesages || Mesages.length <= 0) return;
        alertify.notify("<div>" + Mesages.join("</div><div>") + "</div>", "error", wait, me._NotifyCallback);
    },
    /**
     * 顯示一般提示訊息
     * @param {any} Mesages 訊息內容
     * @param {any} wait 訊息停留秒數
     */
    ShowMsg: function (Mesages, wait) {
        var me = this;
        if (typeof wait === "undefined") wait = 3;
        if (typeof Mesages === "string") {
            alertify.notify(Mesages, "success", wait, me._NotifyCallback);
            return;
        }
        if (!Mesages || Mesages.length <= 0) return;
        alertify.notify("<ul style='padding-left:1em;'><li>" + Mesages.join("</li><li>") + "</li></ul>", "success", wait, me._NotifyCallback);
    },
    /**
     * alert 訊息
     * @param {any} Mesages 訊息內容
     * @param {any} func callback function
     */
    alert: function (Mesages, func) {
        alertify.alert('提示訊息', Mesages, func);
    },
    /**
     * 確認提示
     * @param {any} message 提示訊息
     * @param {any} onok 確認時處理事件
     * @param {any} oncancel 取消時處理事件
     */
    confirm: function (message, onok, oncancel) {
        alertify.confirm('注意!!!', message,
            function () { if (onok) onok(); },
            function () { if (oncancel) oncancel(); }
        );
    }
};
(function () {
    //修正 C# 預設回傳 Json 的日期格式轉換
    var CSharpDate = {
        /**
         * 判定是否為日期格式資料
         * @param {any} _this 待判定資料
         * @returns {boolean} 判斷結果
         */
        IsDateTimeStr: function (_this) {
            if ( /^\/Date\([0-9]+\)\/$/i.test(_this)) return true;
            if (/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(|\.\d{3})/.test(_this) ) return true;
            return false;
        },
        /**
         * 轉換物件成為 Date 物件
         * @param {any} _this 待轉換物件
         * @returns {Date} 日期物件
         */
        ToDate: function (_this) {
            if ( /^\/Date\([0-9]+\)\/$/i.test(_this) ) {
                return new Date(parseInt(_this.substr(6)));
            }
            if (/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(|\.\d{3})/.test(_this) ) {
                return new Date(_this);
            }
            return _this;
        },
        /**
         * 處理修正 C# Json 的日期資料
         * @param {any} OO 待判定物件
         * @returns {any} 修正日期屬性後物件
         */
        Fix: function (OO) {
            if (!OO || typeof OO !== 'object') return;
            var me = this;
            for (var i in OO) {
                var _t = OO[i];
                if (typeof _t === 'object' && _t) {
                    me.Fix(_t);
                }
                if (typeof _t === 'string' && me.IsDateTimeStr(_t)) {
                    OO[i] = me.ToDate(_t);
                }
            }
            return OO;
        }
    };
    //Vue 外掛 ajax 處理引用 axios.js 來處理 ajax 部分
    Vue.prototype.$http = {
        post: function (_cfg) {
            let _csrf = document.querySelector('input[name="__RequestVerificationToken"]');
            let _axioscfg = { headers: {} };
            if (_csrf) _axioscfg.headers["X-CSRF-TOKEN"] = _csrf.value;
            UiHelper.blockUI();
            UiHelper.ClearMsg();
            axios.post(_cfg.url, _cfg.data, _axioscfg)
                .then(function (response) {
                    var _Result = response.data.d ? response.data.d : response.data;
                    UiHelper.unblockUI();
                    if (_cfg.success) {
                        //呼叫日期格式修正處理
                        _Result = CSharpDate.Fix(_Result);
                        _cfg.success(_Result);
                    }
                })
                .catch(function (error) {
                    UiHelper.unblockUI();
                    if (_cfg.fail) {
                        _cfg.fail(error);
                    }
                    if (error.response.status === 400 && error.response.data.errors ) {
                        var _errors = error.response.data.errors;
                        var _msg = [];
                        if (Array.isArray(_errors)) {
                            _msg = _errors;
                        } else {
                            for (let i in _errors) {
                                _msg.push(_errors[i].map(function (x) {
                                    if (x.indexOf('Could not convert string to DateTime') < 0) return x;
                                    return "日期資料格式輸入錯誤";
                                }).join("、"));
                            } 
                        }
                        UiHelper.ShowErr(_msg);
                        return;
                    }
                    if (!_cfg.fail) {
                        UiHelper.ShowErr(_cfg.ApiDesc ? _cfg.ApiDesc + '服務呼叫失敗' : '服務呼叫失敗');
                        console.debug(error);
                    }
                });
        }
    };
    //日期格式化(西元)
    Vue.filter('formatDate', function (value, FormatStr) {
        if (!value) return;
        if (!FormatStr) FormatStr = 'YYYY/MM/DD';
        value = value.getTime ? moment(value.getTime()) : moment(String(value));
        return value.format(FormatStr);
    });
    //日期格式化(民國)
    Vue.filter('CformatDate', function (value, FormatStr) {
        if (!value) return;
        value = value.getTime ? moment(value.getTime()) : moment(String(value));
        if (!FormatStr) FormatStr = 'YYYY/MM/DD';
        FormatStr = FormatStr.replace('YYYY', '_YYYY_');
        return value.format(FormatStr).replace('_' + value.year() + '_', value.year() - 1911);
    });
    //代碼轉換顯示名稱
    Vue.filter('DisplayName', function (value, CodeMap) {
        if (!Array.isArray(CodeMap)) {
            if (CodeMap[String(value)]) return CodeMap[String(value)];
            return "";
        } else {
            let index = parseInt(value);
            if (CodeMap[index]) return CodeMap[index];
            return "";
        }
    });
    //載入 vue 日期選單控制元件
    if (typeof VueFlatpickr !== 'undefined' ) {
        flatpickr.localize(flatpickr.l10ns.zh_tw);
        Vue.component('flat-pickr', VueFlatpickr);
    }
    //調整 alertifyjs 預設參數
    if (typeof alertify !== 'undefined') {
        alertify.set('notifier', 'position', 'top-right');
        alertify.defaults.glossary.ok = "確定";
        alertify.defaults.glossary.cancel = "取消";
    }
})();