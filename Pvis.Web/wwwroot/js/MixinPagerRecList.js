var MixinPagerRecList = {
    data: {
        countOfPage: 10,
        currPage: 1
    },
    computed: {
        pageStart: function () {
            return (this.currPage - 1) * this.countOfPage;
        },
        totalPage: function () {
            return Math.ceil(this.RecList.length / this.countOfPage);
        },
        PageRange: function () {
            var PR = [this.currPage];
            for (var i = 1; i <= 5; i++) {
                var p = this.currPage - i;
                if (p > 0 && p <= this.totalPage) PR.unshift(p);
                if (PR.length >= 5) break;
                p = this.currPage + i;
                if (p > 0 && p <= this.totalPage) PR.push(p);
                if (PR.length >= 5) break;
            }
            if (PR[0] > 1) PR.unshift('..');
            if (PR[PR.length - 1] < this.totalPage) PR.push('..');
            return PR;
        }
    },
    methods: {
        setPage: function (idx) {
            if (isNaN(parseInt(idx))) return;
            if (idx <= 0) {
                this.currPage = 1;
                return;
            }
            if (idx > this.totalPage) {
                this.currPage = this.totalPage;
                return;
            }
            this.currPage = idx;
        }
    }
}