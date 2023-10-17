var MixinPagerRecViewList = {
    data: {
        countViewOfPage: 10,
        currViewPage: 1
    },
    computed: {
        pageViewStart: function () {
            return (this.currViewPage - 1) * this.countViewOfPage;
        },
        totalViewPage: function () {
            return Math.ceil(this.RecSchSBList.length / this.countViewOfPage);
        },
        PageViewRange: function () {
            var PR = [this.currViewPage];
            for (var i = 1; i <= 5; i++) {
                var p = this.currViewPage - i;
                if (p > 0 && p <= this.totalViewPage) PR.unshift(p);
                if (PR.length >= 5) break;
                p = this.currViewPage + i;
                if (p > 0 && p <= this.totalViewPage) PR.push(p);
                if (PR.length >= 5) break;
            }
            if (PR[0] > 1) PR.unshift('..');
            if (PR[PR.length - 1] < this.totalViewPage) PR.push('..');
            return PR;
        }
    },
    methods: {
        setViewPage: function (idx) {
            if (isNaN(parseInt(idx))) return;
            if (idx <= 0) {
                this.currViewPage = 1;
                return;
            }
            if (idx > this.totalViewPage) {
                this.currViewPage = this.totalViewPage;
                return;
            }
            this.currViewPage = idx;
        }
    }
}