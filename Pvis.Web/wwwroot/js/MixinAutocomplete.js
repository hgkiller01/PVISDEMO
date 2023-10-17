var MixinAutocomplete = {
    components: {
        'Autocomplete': Autocomplete
    },
    methods: {
        handleInput: function (e, v, f) {
            v[f] = e.target.value;
            this.$forceUpdate();
        },
        search: function (input) {
            var me = this;
            if (input.length < 1) { return [] }
            input = input.replace(/台/, "臺");
            return me.Towns.filter(function (Town) {
                return Town.indexOf(input) > -1;
            });
        },
    }
}