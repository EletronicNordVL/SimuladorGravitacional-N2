import assert from 'node:assert/strict';

const base = 'http://localhost:5080';
let checks = 0;
function check(condition, message) { assert.ok(condition, message); checks++; console.log(`OK ${message}`); }
async function request(path, data, method = 'POST') {
  return fetch(base + path, { method, headers: { 'Content-Type': 'application/json' }, body: data === undefined ? undefined : JSON.stringify(data) });
}
async function create(count = 12) { const r = await request('/api/universes/create', { count }); assert.equal(r.status, 200); return r.json(); }

const page = await fetch(base);
check(page.ok && (await page.text()).includes('A gravidade em movimento'), 'Página inicial servida localmente');
for (const asset of ['styles.css', 'app.js', 'icon.svg']) check((await fetch(`${base}/${asset}`)).ok, `Asset ${asset} disponível`);
const orbital = await create();
check(orbital.corpos.length === 12, 'Universo inicia com a quantidade de corpos aleatórios solicitada');
check(orbital.corpos.every(c => c.massa >= 1e22 && c.massa <= 1e25 && c.densidade >= 3000 && c.densidade <= 6000 && Math.abs(c.posX) <= 5e9 && Math.abs(c.posY) <= 5e9 && Math.abs(c.velX) <= 200 && Math.abs(c.velY) <= 200), 'Faixas aleatórias correspondem exatamente ao projeto original');
check(orbital.corpos.every(c => c.massa > 0 && c.raio > 0), 'Massas e raios físicos válidos');
let updated = await (await request('/api/simulation/step', { state: orbital, steps: 10 })).json();
check(updated.iteration === 10 && updated.elapsed === 36000, 'Contadores avançam pelo intervalo físico');
check(updated.corpos[1].posX !== orbital.corpos[1].posX, 'Corpos evoluem pelo motor C#');
// Estado analítico exclusivo do teste, para verificar a física sem depender do sorteio.
const binary = { quantidadeIteracoes: 10000, tempoEntreIteracoes: 300, iteration: 0, elapsed: 0,
  corpos: [
    { nome: 'Teste A', massa: 1e28, densidade: 1400, posX: -9e8, posY: 0, velX: 0, velY: -Math.sqrt(6.674184e-11 * 1e28 / 3.6e9) },
    { nome: 'Teste B', massa: 1e28, densidade: 1400, posX: 9e8, posY: 0, velX: 0, velY: Math.sqrt(6.674184e-11 * 1e28 / 3.6e9) }
  ] };
const forceState = structuredClone(binary); forceState.quantidadeIteracoes = 1; forceState.iteration = 1;
const calculated = await (await request('/api/simulation/step', { state: forceState, steps: 1 })).json();
const force = calculated.corpos[0].forcaX;
const expected = 6.674184e-11 * 1e28 * 1e28 / (1.8e9 ** 2);
check(Math.abs(force / expected - 1) < 1e-12 && force === -calculated.corpos[1].forcaX, 'Gravitação de Newton e ação/reação');
updated = await (await request('/api/simulation/step', { state: binary, steps: 20 })).json();
check(Math.abs(updated.corpos[0].posX + updated.corpos[1].posX) < 1e-5 && Math.abs(updated.corpos[0].velY + updated.corpos[1].velY) < 1e-5, 'Simetria preservada na evolução binária');
check((await create(30)).corpos.length === 30, 'Geração aleatória respeita quantidade personalizada');
const one = await create(1);
const oneNext = await (await request('/api/simulation/step', { state: one, steps: 1 })).json();
check(Math.abs(oneNext.corpos[0].posX - (one.corpos[0].posX + one.corpos[0].velX * one.tempoEntreIteracoes)) < 1e-4, 'Um corpo sem força segue movimento uniforme');
const limited = structuredClone(binary); limited.quantidadeIteracoes = 3;
const finished = await (await request('/api/simulation/step', { state: limited, steps: 10 })).json();
check(finished.iteration === 3 && finished.elapsed === 900, 'Limite de iterações respeitado');
const collision = structuredClone(binary);
collision.tempoEntreIteracoes = .001;
collision.corpos = [
  { nome: 'A', massa: 100, densidade: 1, posX: -1, posY: 0, velX: 1, velY: 0 },
  { nome: 'B', massa: 100, densidade: 1, posX: 1, posY: 0, velX: -1, velY: 0 }
];
const collided = await (await request('/api/simulation/step', { state: collision, steps: 1 })).json();
check(collided.corpos[0].velX < 0 && collided.corpos[1].velX > 0, 'Colisão elástica calculada pelo motor original');
const exported = await request('/api/universes/export', orbital);
check(exported.ok && exported.headers.get('content-disposition').includes('universo_exportado.txt'), 'Exportação de arquivo compatível');
const text = await exported.text();
const imported = await fetch(base + '/api/universes/import', { method: 'POST', body: text });
const restored = await imported.json();
check(imported.ok && restored.corpos[3].massa === orbital.corpos[3].massa && restored.corpos[3].posX === orbital.corpos[3].posX, 'Exportação e importação preservam valores');
const comma = await fetch(base + '/api/universes/import', { method: 'POST', body: text.replaceAll('.', ',') });
check(comma.ok, 'Importação aceita vírgula decimal');
check((await fetch(base + '/api/universes/import', { method: 'POST', body: 'arquivo inválido' })).status === 400, 'Arquivo inválido rejeitado');
check((await request('/api/universes/create', { count: 101 })).status === 400, 'Limite de corpos validado');
const invalid = structuredClone(binary); invalid.tempoEntreIteracoes = 0;
check((await request('/api/simulation/step', { state: invalid, steps: 1 })).status === 400, 'Passo de tempo inválido rejeitado');
invalid.tempoEntreIteracoes = 300; invalid.corpos[0].nome = 'in;válido';
check((await request('/api/universes/save', invalid)).status === 400, 'Nome incompatível com arquivo rejeitado antes de salvar');
check((await request('/api/simulation/step', { state: binary, steps: 101 })).status === 400, 'Lote excessivo rejeitado');
check((await fetch(base + '/api/universes/arquivo.txt')).status === 400, 'Acesso restrito aos nomes de universo');
check((await fetch(base + '/api/universes')).ok, 'Biblioteca de universos disponível');
console.log(`\n${checks} verificações passaram.`);
