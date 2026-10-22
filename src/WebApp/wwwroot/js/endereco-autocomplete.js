let timerDebounce;

const estadosUF = {
    'Acre': 'AC', 'Alagoas': 'AL', 'Amapá': 'AP', 'Amazonas': 'AM', 'Bahia': 'BA',
    'Ceará': 'CE', 'Distrito Federal': 'DF', 'Espírito Santo': 'ES', 'Goiás': 'GO',
    'Maranhão': 'MA', 'Mato Grosso': 'MT', 'Mato Grosso do Sul': 'MS', 'Minas Gerais': 'MG',
    'Pará': 'PA', 'Paraíba': 'PB', 'Paraná': 'PR', 'Pernambuco': 'PE', 'Piauí': 'PI',
    'Rio de Janeiro': 'RJ', 'Rio Grande do Norte': 'RN', 'Rio Grande do Sul': 'RS',
    'Rondônia': 'RO', 'Roraima': 'RR', 'Santa Catarina': 'SC', 'São Paulo': 'SP',
    'Sergipe': 'SE', 'Tocantins': 'TO'
};

function obterSiglaUF(estado) {
    if (!estado) return '';
    if (estado.length === 2) return estado.toUpperCase();
    return estadosUF[estado] || estado;
}

function formatarCoordenada(valor) {
    if (valor === null || valor === undefined || valor === '') return '';
    const num = parseFloat(valor);
    return isNaN(num) ? '' : num.toFixed(7);
}

function abrirNoMapa() {
    const lat = document.getElementById('latInput')?.value;
    const lng = document.getElementById('lngInput')?.value;

    if (lat && lng) {
        window.open(`https://www.google.com/maps?q=${lat},${lng}`, '_blank');
        return;
    }

    const logradouro = document.getElementById('logradouroInput')?.value || '';
    const numero = document.getElementById('numeroInput')?.value || '';
    const bairro = document.getElementById('bairroInput')?.value || '';
    const cidade = document.getElementById('cidadeInput')?.value || '';
    const uf = document.getElementById('ufInput')?.value || '';

    const enderecoCompleto = [logradouro, numero, bairro, cidade, uf, "Brasil"].filter(Boolean).join(", ");

    if (enderecoCompleto.length > 10) {
        window.open(`https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(enderecoCompleto)}`, '_blank');
    } else {
        alert('Preencha as coordenadas ou o endereço para visualizar no mapa.');
    }
}

async function buscarCoordenadasPorEndereco() {
    const status = document.getElementById('geoStatus');

    const cep = document.getElementById('cepInput')?.value || '';
    const logradouro = document.getElementById('logradouroInput')?.value || '';
    const numero = document.getElementById('numeroInput')?.value || '';
    const bairro = document.getElementById('bairroInput')?.value || '';
    const cidade = document.getElementById('cidadeInput')?.value || '';
    const uf = document.getElementById('ufInput')?.value || '';

    if (!logradouro && !cep) {
        alert('Por favor, preencha ao menos o Logradouro ou CEP para buscar as coordenadas.');
        return;
    }

    if (status) {
        status.innerHTML = '<span class="text-info"><i class="spinner-border spinner-border-sm me-1"></i> Buscando coordenadas...</span>';
    }

    try {
        const params = new URLSearchParams({
            cep: cep,
            logradouro: logradouro,
            numero: numero,
            bairro: bairro,
            cidade: cidade,
            uf: uf
        });

        const response = await fetch(`?handler=BuscarCoordenadas&${params.toString()}`);
        const data = await response.json();

        if (data.success) {
            const latStr = formatarCoordenada(data.lat);
            const lngStr = formatarCoordenada(data.lng);

            const latInput = document.getElementById('latInput');
            const lngInput = document.getElementById('lngInput');

            if (latInput) latInput.value = latStr;
            if (lngInput) lngInput.value = lngStr;

            if (status) {
                status.innerHTML = `<span class="text-success fw-bold">✓ Coordenadas via ${data.fonte} (${latStr}, ${lngStr})</span>`;
            }
        } else {
            if (status) status.innerHTML = `<span class="text-danger">⚠️ ${data.message}</span>`;
        }
    } catch (error) {
        console.error('Erro na requisição:', error);
        if (status) status.innerHTML = '<span class="text-danger">⚠️ Erro ao consultar o serviço de mapas.</span>';
    }
}

function capturarGeolocalizacao() {
    const status = document.getElementById('geoStatus');
    if (!navigator.geolocation) {
        if (status) status.innerText = 'GPS não suportado pelo navegador.';
        return;
    }

    if (status) status.innerText = 'Obtendo GPS...';

    navigator.geolocation.getCurrentPosition(
        (pos) => {
            const lat = formatarCoordenada(pos.coords.latitude);
            const lng = formatarCoordenada(pos.coords.longitude);

            const latInput = document.getElementById('latInput');
            const lngInput = document.getElementById('lngInput');

            if (latInput) latInput.value = lat;
            if (lngInput) lngInput.value = lng;

            if (status) {
                status.innerHTML = `<span class="text-success fw-bold">✓ GPS Capturado! (${lat}, ${lng})</span>`;
            }
        },
        (err) => {
            if (status) status.innerHTML = `<span class="text-danger">Erro ao obter GPS.</span>`;
        },
        { enableHighAccuracy: true, timeout: 5000 }
    );
}

function buscarSugestoesEndereco(termo) {
    clearTimeout(timerDebounce);
    const ul = document.getElementById('listaSugestoes');

    if (!termo || termo.trim().length < 3) {
        if (ul) {
            ul.style.display = 'none';
            ul.innerHTML = '';
        }
        return;
    }

    timerDebounce = setTimeout(async () => {
        try {
            const termoConsulta = `${termo.trim()}, Brasil`;
            const urlPhoton = `https://photon.komoot.io/api/?q=${encodeURIComponent(termoConsulta)}&limit=5&lang=default`;

            let response = await fetch(urlPhoton);
            let data = await response.json();

            let resultados = data.features || [];

            if (resultados.length === 0) {
                const urlNominatim = `https://nominatim.openstreetmap.org/search?format=json&addressdetails=1&countrycodes=br&limit=5&q=${encodeURIComponent(termo)}`;
                const resNom = await fetch(urlNominatim, { headers: { 'Accept-Language': 'pt-BR' } });
                const dataNom = await resNom.json();

                resultados = dataNom.map(item => ({
                    geometry: { coordinates: [parseFloat(item.lon), parseFloat(item.lat)] },
                    properties: {
                        street: item.address.road || item.address.pedestrian || item.address.suburb || '',
                        housenumber: item.address.house_number || '',
                        district: item.address.suburb || item.address.neighbourhood || item.address.quarter || '',
                        city: item.address.city || item.address.town || item.address.municipality || item.address.village || '',
                        state: item.address.state || '',
                        postcode: item.address.postcode || ''
                    }
                }));
            }

            if (!ul) return;
            ul.innerHTML = '';

            if (resultados.length === 0) {
                ul.style.display = 'none';
                return;
            }

            resultados.forEach(item => {
                const props = item.properties;
                const coords = item.geometry.coordinates;

                const rua = props.street || props.name || '';
                const bairro = props.district || props.suburb || props.neighbourhood || props.quarter || props.hamlet || '';
                const cidade = props.city || props.town || props.county || props.municipality || '';
                const uf = obterSiglaUF(props.state);
                const cep = props.postcode || '';

                const titulo = [rua, props.housenumber].filter(Boolean).join(', ') || props.name;
                const subtitulo = [bairro, cidade, uf].filter(Boolean).join(' - ');

                const li = document.createElement('li');
                li.className = 'dropdown-item p-2 border-bottom';
                li.style.cursor = 'pointer';
                li.innerHTML = `
                    <div class="d-flex align-items-center">
                        <i class="bi bi-geo-alt-fill text-primary me-2 fs-5"></i>
                        <div>
                            <strong class="d-block text-dark small">${titulo}</strong>
                            <small class="text-muted d-block" style="font-size: 0.75rem;">${subtitulo}</small>
                        </div>
                    </div>
                `;

                li.onclick = () => selecionarEndereco({
                    logradouro: rua,
                    numero: props.housenumber || '',
                    bairro: bairro,
                    cidade: cidade,
                    uf: uf,
                    cep: cep,
                    lat: coords[1],
                    lng: coords[0],
                    textoExibicao: `${titulo}${subtitulo ? ' - ' + subtitulo : ''}`
                });

                ul.appendChild(li);
            });

            ul.style.display = 'block';
        } catch (error) {
            console.error('Erro na busca de sugestões:', error);
        }
    }, 350);
}

function selecionarEndereco(item) {
    const autocompleteInput = document.getElementById('enderecoAutocomplete');
    const logradouroInput = document.getElementById('logradouroInput');
    const numeroInput = document.getElementById('numeroInput');
    const bairroInput = document.getElementById('bairroInput');
    const cidadeInput = document.getElementById('cidadeInput');
    const ufInput = document.getElementById('ufInput');
    const cepInput = document.getElementById('cepInput');
    const latInput = document.getElementById('latInput');
    const lngInput = document.getElementById('lngInput');
    const status = document.getElementById('geoStatus');

    if (autocompleteInput) autocompleteInput.value = item.textoExibicao;
    if (logradouroInput) logradouroInput.value = item.logradouro;
    if (numeroInput && item.numero) numeroInput.value = item.numero;
    if (bairroInput) bairroInput.value = item.bairro;
    if (cidadeInput) cidadeInput.value = item.cidade;
    if (ufInput) ufInput.value = item.uf;
    if (cepInput && item.cep) cepInput.value = item.cep;

    const lat = formatarCoordenada(item.lat);
    const lng = formatarCoordenada(item.lng);

    if (latInput) latInput.value = lat;
    if (lngInput) lngInput.value = lng;

    if (status) {
        status.innerHTML = `<span class="text-success fw-bold">✓ Localização Selecionada! (${lat}, ${lng})</span>`;
    }

    const ul = document.getElementById('listaSugestoes');
    if (ul) {
        ul.style.display = 'none';
        ul.innerHTML = '';
    }
}

// Fecha o menu de sugestões ao clicar fora do componente
document.addEventListener('click', function (e) {
    const ul = document.getElementById('listaSugestoes');
    const input = document.getElementById('enderecoAutocomplete');
    if (input && ul && !input.contains(e.target) && !ul.contains(e.target)) {
        ul.style.display = 'none';
    }
});


// Função auxiliar para limpar todos os campos antes do preenchimento
function limparCamposEndereco(manterCep = false, manterLogradouro = false) {
    if (!manterCep) document.getElementById('cepInput').value = '';
    if (!manterLogradouro) document.getElementById('logradouroInput').value = '';

    document.getElementById('numeroInput').value = '';
    document.getElementById('complementoInput').value = '';
    document.getElementById('bairroInput').value = '';
    document.getElementById('cidadeInput').value = '';
    document.getElementById('ufInput').value = '';
    document.getElementById('latInput').value = '';
    document.getElementById('lngInput').value = '';
    document.getElementById('enderecoAutocomplete').value = '';
}

// 1. Busca disparada pelo botão ao lado do CEP
async function buscarPorCep() {
    const status = document.getElementById('geoStatus');
    const cepInput = document.getElementById('cepInput');
    const cep = cepInput?.value?.trim() || '';

    if (!cep) {
        alert('Por favor, informe o CEP para realizar a busca.');
        return;
    }

    // Guarda o CEP e limpa todos os outros campos da tela
    limparCamposEndereco(true, false);

    status.innerHTML = '<span class="text-info"><i class="spinner-border spinner-border-sm me-1"></i> Buscando endereço por CEP...</span>';

    try {
        const params = new URLSearchParams({ cep: cep });
        const response = await fetch(`?handler=BuscarCoordenadas&${params.toString()}`);
        const data = await response.json();

        if (data.success) {
            if (data.logradouro) document.getElementById('logradouroInput').value = data.logradouro;
            if (data.bairro) document.getElementById('bairroInput').value = data.bairro;
            if (data.cidade) document.getElementById('cidadeInput').value = data.cidade;
            if (data.uf) document.getElementById('ufInput').value = data.uf;
            if (data.cep) document.getElementById('cepInput').value = data.cep;

            const latStr = formatarCoordenada(data.lat);
            const lngStr = formatarCoordenada(data.lng);
            document.getElementById('latInput').value = latStr;
            document.getElementById('lngInput').value = lngStr;

            const resumo = [data.logradouro, data.bairro, data.cidade, data.uf].filter(Boolean).join(', ');
            document.getElementById('enderecoAutocomplete').value = resumo;

            status.innerHTML = `<span class="text-success fw-bold">✓ Endereço carregado pelo CEP (${latStr || 'Sem GPS'}, ${lngStr || 'Sem GPS'})</span>`;
        } else {
            status.innerHTML = `<span class="text-danger">⚠️ ${data.message}</span>`;
        }
    } catch (error) {
        console.error('Erro na requisição:', error);
        status.innerHTML = '<span class="text-danger">⚠️ Erro ao consultar o serviço de CEP.</span>';
    }
}

// 2. Busca disparada pelo botão ao lado do Logradouro
async function buscarPorLogradouro() {
    const status = document.getElementById('geoStatus');
    const logradouroInput = document.getElementById('logradouroInput');
    const logradouro = logradouroInput?.value?.trim() || '';
    const cidade = document.getElementById('cidadeInput')?.value || 'Magé';
    const uf = document.getElementById('ufInput')?.value || 'RJ';

    if (!logradouro) {
        alert('Por favor, informe a Rua/Logradouro para realizar a busca.');
        return;
    }

    // Guarda o Logradouro e limpa todos os outros campos (inclusive CEP)
    limparCamposEndereco(false, true);

    status.innerHTML = '<span class="text-info"><i class="spinner-border spinner-border-sm me-1"></i> Buscando coordenadas e dados do endereço...</span>';

    try {
        const params = new URLSearchParams({
            logradouro: logradouro,
            cidade: cidade,
            uf: uf
        });

        const response = await fetch(`?handler=BuscarCoordenadas&${params.toString()}`);
        const data = await response.json();

        if (data.success) {
            if (data.logradouro) document.getElementById('logradouroInput').value = data.logradouro;
            if (data.bairro) document.getElementById('bairroInput').value = data.bairro;
            if (data.cidade) document.getElementById('cidadeInput').value = data.cidade;
            if (data.uf) document.getElementById('ufInput').value = data.uf;
            if (data.cep) document.getElementById('cepInput').value = data.cep;

            const latStr = formatarCoordenada(data.lat);
            const lngStr = formatarCoordenada(data.lng);
            document.getElementById('latInput').value = latStr;
            document.getElementById('lngInput').value = lngStr;

            const resumo = [data.logradouro, data.bairro, data.cidade, data.uf].filter(Boolean).join(', ');
            document.getElementById('enderecoAutocomplete').value = resumo;

            status.innerHTML = `<span class="text-success fw-bold">✓ Coordenadas localizadas (${latStr}, ${lngStr})</span>`;
        } else {
            status.innerHTML = `<span class="text-danger">⚠️ ${data.message}</span>`;
        }
    } catch (error) {
        console.error('Erro na requisição:', error);
        status.innerHTML = '<span class="text-danger">⚠️ Erro ao consultar serviço de mapas.</span>';
    }
}