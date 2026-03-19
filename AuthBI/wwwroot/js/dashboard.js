const { createApp, nextTick } = Vue;

createApp({
    data() {
        return {
            dataset: [], // dados originais da API
            filtros: { nivel0: [], nivel1: [], nivel2: [], nivel3: [], contaAnalitica: [] },
            selecionados: { nivel0: [], nivel1: [], nivel2: [], nivel3: [], contaAnalitica: [] },
            dadosTabela: [], // dados filtrados
            multiSelects: {},
            grid: null
        };
    },

    async mounted() {
        // 1️⃣ Carregar dados da API
        await this.carregarDataset();

        // 2️⃣ Montar filtros com base no dataset
        this.montarFiltros();

        // 3️⃣ Montar tabela inicial
        this.montarTabela();

        // 4️⃣ Inicializar grid
        this.inicializarGrid();

        // 5️⃣ Espera DOM renderizar
        await nextTick();

        // 6️⃣ Inicializar MultiSelects
        this.inicializarMultiSelects();
    },

    methods: {
        // =====================
        // CARREGAR DADOS
        // =====================
        async carregarDataset() {
            try {
                const res = await fetch('/api/dashboard/dados', { credentials: 'include' });
                const data = await res.json();
                this.dataset = Array.isArray(data) ? [...data] : [];
                console.log('Dataset carregado:', this.dataset);
            } catch (err) {
                console.error('Erro ao carregar dados da API:', err);
            }
        },

        // =====================
        // MONTAR FILTROS
        // =====================
        montarFiltros() {
            const d = this.dataset;

            // 🔹 Use os nomes exatos do JSON vindo da API
            this.filtros.nivel0 = [...new Set(d.map(x => x.nivel0).filter(Boolean))];
            this.filtros.nivel1 = [...new Set(d.map(x => x.nivel1).filter(Boolean))];
            this.filtros.nivel2 = [...new Set(d.map(x => x.nivel2).filter(Boolean))];
            this.filtros.nivel3 = [...new Set(d.map(x => x.nivel3).filter(Boolean))];
            this.filtros.contaAnalitica = [...new Set(d.map(x => x.contaAnalitica).filter(Boolean))];

            console.log('Filtros montados:', this.filtros);
        },

        montarTabela() {
            this.dadosTabela = [...this.dataset];
        },

        mapCampo(campo) {
            return {
                nivel0: 'nivel0',
                nivel1: 'nivel1',
                nivel2: 'nivel2',
                nivel3: 'nivel3',
                contaAnalitica: 'contaAnalitica'
            }[campo];
        },

        // =====================
        // APLICAR FILTROS
        // =====================
        aplicarFiltros() {
            let dados = this.dataset;

            Object.keys(this.selecionados).forEach(filtro => {
                const vals = this.selecionados[filtro];
                if (vals.length) {
                    dados = dados.filter(d => vals.includes(d[this.mapCampo(filtro)]));
                }
            });

            this.dadosTabela = dados;
            this.atualizarGrid();
        },

        // =====================
        // INICIALIZAR MULTISELECTS
        // =====================
        inicializarMultiSelects() {
            ['nivel0', 'nivel1', 'nivel2', 'nivel3', 'contaAnalitica'].forEach(id => {
                const data = this.filtros[id].length ? [...this.filtros[id]] : ['Nenhum registro'];
                const val = this.selecionados[id].length ? [...this.selecionados[id]] : [];

                const ms = new ej.dropdowns.MultiSelect({
                    dataSource: data,
                    value: val,
                    mode: 'Box',
                    placeholder: `Selecione ${id.charAt(0).toUpperCase() + id.slice(1)}`,
                    allowFiltering: true,
                    showClearButton: true,
                    change: (e) => {
                        this.selecionados[id] = e.value;
                        this.aplicarFiltros();
                    }
                });

                ms.appendTo(`#${id}`);
                this.multiSelects[id] = ms;
            });
        },

        // =====================
        // GRID
        // =====================
        inicializarGrid() {
            this.grid = new ej.grids.Grid({
                dataSource: this.dadosTabela,
                height: 400,
                allowSorting: true,
                allowFiltering: true,
                enableHover: true,
                enableAltRow: true,
                rowHeight: 40,
                allowResizing: true,
                columns: [
                    { field: 'nivel0', headerText: 'Nível 0', width: 150 },
                    { field: 'nivel1', headerText: 'Nível 1', width: 150 },
                    { field: 'nivel2', headerText: 'Nível 2', width: 150 },
                    { field: 'nivel3', headerText: 'Nível 3', width: 150 },
                    { field: 'contaAnalitica', headerText: 'Conta Analítica', width: 200 }
                ]
            });

            this.grid.appendTo('#grid');
        },

        atualizarGrid() {
            if (this.grid) {
                this.grid.dataSource = this.dadosTabela;
                this.grid.refresh();
            }
        }
    }
}).mount('#app');
