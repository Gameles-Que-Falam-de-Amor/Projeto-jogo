/* ═══════════════════════════════════════════════════════════════════════════
   CHRONICLES RPG — FRONT-END ENGINE
   Máquina de estados de telas · API calls · Render por fase
═══════════════════════════════════════════════════════════════════════════ */

// ─── CONSTANTES ───────────────────────────────────────────────────────────
const API = {
  novo:      '/api/jogo/novo',
  estado:    '/api/jogo/estado',
  personagem:'/api/jogo/personagem',
  avancar:   '/api/jogo/avancar',
  combate:   '/api/jogo/combate',
  acao:      '/api/jogo/acao',
  evento:    '/api/jogo/evento',
};

const SCREENS = {
  intro:            document.getElementById('screen-intro'),
  creation:         document.getElementById('screen-creation'),
  map:              document.getElementById('screen-map'),
  encounter:        document.getElementById('screen-encounter'),
  combat:           document.getElementById('screen-combat'),
  event:            document.getElementById('screen-event'),
  victory:          document.getElementById('screen-victory'),
  final:            document.getElementById('screen-final'),
};

// Fase do backend → id da tela
const FASE_TO_SCREEN = {
  criacao:         'creation',
  mapa:            'map',
  encontro:        'encounter',
  combate:         'combat',
  vitoria_estagio: 'victory',
  evento:          'event',
  descanso:        'event',
  loja:            'event',
  cutscene:        'event',
  fim:             'final',
};

// ─── ESTADO LOCAL ─────────────────────────────────────────────────────────
let state           = null;   // último JSON do backend
let classeEscolhida = null;   // classe pendente de nome
let nivelAnterior   = 1;      // detectar level-up
const logCache      = [];
let _autoEndTimer   = null;   // timer de encerramento automático

// ─── UTILITÁRIOS HTTP ─────────────────────────────────────────────────────
async function post(url, body = {}) {
  const r = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });
  if (!r.ok) throw new Error(`Erro ${r.status}`);
  return r.json();
}

// ─── MÁQUINA DE TELAS ─────────────────────────────────────────────────────
let currentScreen = 'intro';

function showScreen(id, instant = false) {
  if (id === currentScreen && SCREENS[id].classList.contains('active')) return;

  const prev = SCREENS[currentScreen];
  const next = SCREENS[id];
  if (!next) return;

  // Saída da tela anterior
  if (prev && prev !== next) {
    prev.classList.add('exit');
    setTimeout(() => {
      prev.classList.remove('active', 'exit');
    }, instant ? 0 : 500);
  }

  // Entrada da nova tela
  setTimeout(() => {
    next.classList.add('active');
  }, instant ? 0 : 80);

  currentScreen = id;
}

// ─── RENDER PRINCIPAL ─────────────────────────────────────────────────────
function render(s) {
  state = s;
  const screenId = FASE_TO_SCREEN[s.fase] || 'intro';

  // Vitória: não transitar imediatamente — mostrar resultado por 2s
  if (s.fase === 'vitoria_estagio' && currentScreen === 'combat') {
    clearAutoEnd();
    renderCombat(s); // mostra HP inimigo em 0 na tela de combate
    setTimeout(() => {
      renderVictory(s);
      showScreen('victory');
    }, 2000);
    return;
  }

  switch (s.fase) {
    case 'criacao':         renderCreation(s); break;
    case 'mapa':            renderMap(s);      break;
    case 'encontro':        renderEncounter(s);break;
    case 'combate':         renderCombat(s);   break;
    case 'vitoria_estagio': renderVictory(s);  break;
    case 'descanso':
    case 'loja':
    case 'cutscene':
    case 'evento':          renderEvent(s);    break;
    case 'fim':             renderFinal(s);    break;
  }

  showScreen(screenId);
}

// ─── ENCERRAMENTO AUTOMÁTICO DE TURNO ─────────────────────────────────────
function clearAutoEnd() {
  if (_autoEndTimer) { clearTimeout(_autoEndTimer); _autoEndTimer = null; }
  const bar = document.getElementById('auto-end-bar');
  if (bar) bar.classList.add('hidden');
  // Remove pulsação do botão encerrar
  const btnEnc = document.getElementById('btn-encerrar');
  if (btnEnc) btnEnc.classList.remove('pronto');
}

function scheduleAutoEnd() {
  clearAutoEnd();

  // Mostra barra de progresso
  const bar = document.getElementById('auto-end-bar');
  const fill = document.getElementById('auto-end-fill');
  const lbl = document.getElementById('auto-end-label');
  if (bar && fill) {
    bar.classList.remove('hidden');
    // Reinicia a animação CSS
    fill.style.animation = 'none';
    fill.offsetWidth; // reflow
    fill.style.animation = '';
    lbl.textContent = 'Turno do inimigo em 2s...';
  }

  // Destaca o botão encerrar
  const btnEnc = document.getElementById('btn-encerrar');
  if (btnEnc) btnEnc.classList.add('pronto');

  _autoEndTimer = setTimeout(async () => {
    clearAutoEnd();
    if (state?.fase !== 'combate') return;
    const prev = state;
    const s = await post(API.acao, { acao: 'encerrar' });
    if (s.jogador && prev?.jogador && s.jogador.hpAtual < prev.jogador.hpAtual) shakeCombatant('player');
    if (s.inimigo && prev?.inimigo && s.inimigo.hpAtual < prev.inimigo.hpAtual) shakeCombatant('enemy');
    render(s);
  }, 2000);
}

// ─── TELA 2: CRIAÇÃO ──────────────────────────────────────────────────────
function renderCreation(s) {
  const grid = document.getElementById('class-grid');
  if (!s.classes) return;

  grid.innerHTML = '';
  s.classes.forEach(cl => {
    const card = document.createElement('article');
    card.className = 'class-card';
    card.innerHTML = `
      <img class="class-card-img" src="${cl.imagem}" alt="${cl.nome}" />
      <div class="class-card-body">
        <div class="class-card-name">${cl.nome}</div>
        <div class="class-card-archetype">${cl.arquetipo}</div>
        <p class="class-card-desc">${cl.descricao}</p>
        <div class="class-card-skills">
          ${(cl.habilidades || []).map(h => `<span class="skill-tag">${h}</span>`).join('')}
        </div>
        <div class="class-card-meta">
          <span>❤ ${cl.hp}</span>
          <span>🛡 ${cl.armadura}</span>
          <span>🎲 ${cl.dado}</span>
        </div>
      </div>
    `;
    card.addEventListener('click', () => openNameModal(cl));
    grid.appendChild(card);
  });
}

function openNameModal(cl) {
  classeEscolhida = cl;
  document.getElementById('name-modal-title').textContent = `Nomear ${cl.nome}`;
  document.getElementById('name-modal-class').textContent  = cl.arquetipo;
  const input = document.getElementById('name-input');
  input.value = '';
  input.placeholder = `Nome do ${cl.nome}...`;
  document.getElementById('name-modal').classList.remove('hidden');
  setTimeout(() => input.focus(), 50);
}

document.getElementById('btn-cancel-name').addEventListener('click', () => {
  document.getElementById('name-modal').classList.add('hidden');
  classeEscolhida = null;
});

document.getElementById('btn-confirm-name').addEventListener('click', confirmarPersonagem);
document.getElementById('name-input').addEventListener('keydown', e => {
  if (e.key === 'Enter') confirmarPersonagem();
});

async function confirmarPersonagem() {
  if (!classeEscolhida) return;
  const nome = document.getElementById('name-input').value.trim();
  document.getElementById('name-modal').classList.add('hidden');
  nivelAnterior = 1;
  render(await post(API.personagem, { classe: classeEscolhida.id, nome }));
  classeEscolhida = null;
}

// ─── TELA 3: MAPA ────────────────────────────────────────────────────────
function renderMap(s) {
  if (!s.jogador) return;
  const j = s.jogador;

  // HUD
  document.getElementById('hud-portrait').style.backgroundImage = `url("${imgClasse(j.classe)}")`;
  document.getElementById('hud-name').textContent    = j.nome;
  document.getElementById('hud-level').textContent   = `Nível ${j.nivel}`;
  document.getElementById('hud-hp-text').textContent = `HP ${j.hpAtual}/${j.hpMaximo}`;
  document.getElementById('hud-xp-text').textContent = `XP ${j.xp}/${j.xpProximoNivel}`;
  document.getElementById('hud-ca').textContent       = `CA ${j.classeArmadura}`;
  document.getElementById('hud-ouro').textContent     = `${j.ouro} 🪙`;
  document.getElementById('hud-hp-fill').style.width  = pct(j.hpAtual, j.hpMaximo) + '%';

  // Mapa de estágios
  const mapEl = document.getElementById('stage-map');
  mapEl.innerHTML = '';
  (s.mapa || []).forEach(est => {
    const node = document.createElement('div');
    let cls = 'stage-node';
    if (est.passado)                                    cls += ' passado';
    if (est.atual)                                      cls += ' atual';
    if (est.categoria === 'Boss Final')                 cls += ' boss-final';
    else if (est.categoria === 'Mini-Boss')             cls += ' boss';

    const emoji = est.categoria === 'Boss Final' ? '☠'
                : est.categoria === 'Mini-Boss'  ? '⚔'
                : est.passado                    ? '✓'
                : '';

    node.className = cls;
    node.innerHTML = `
      <div class="stage-node-num">${emoji || est.numero}</div>
      <div class="stage-node-info">
        <div class="stage-node-nome">${est.nome}</div>
        <div class="stage-node-cat">${est.categoria} · ND ${est.nd}</div>
      </div>
      ${est.atual ? '<span class="stage-node-atual-badge">Próximo</span>' : ''}
    `;
    mapEl.appendChild(node);

    // Scroll para o estágio atual
    if (est.atual) setTimeout(() => node.scrollIntoView({ behavior:'smooth', block:'center' }), 200);
  });

  // Botão avançar
  const botao = document.getElementById('btn-avancar');
  const inimigo = (s.mapa || []).find(e => e.atual);
  if (inimigo) {
    document.getElementById('btn-avancar-text').textContent =
      inimigo.categoria === 'Boss Final'  ? `⚠ Enfrentar ${inimigo.nome} (Boss Final)` :
      inimigo.categoria === 'Mini-Boss'   ? `⚔ Enfrentar ${inimigo.nome} (Mini-Boss)` :
                                            `Avançar → Estágio ${s.estagioAtual}`;
  }
}

// ─── TELA 4: ENCONTRO ────────────────────────────────────────────────────
function renderEncounter(s) {
  if (!s.inimigo) return;
  const ini = s.inimigo;
  const ehBoss = ini.boss;

  document.getElementById('encounter-stage').textContent = `Estágio ${s.estagioAtual} de ${s.estagioMax}`;
  document.getElementById('encounter-name').textContent  = ini.nome;
  document.getElementById('encounter-desc').textContent  = ini.descricao || '';
  document.getElementById('enc-hp').textContent          = `HP: ${ini.hpMaximo}`;
  document.getElementById('enc-ca').textContent          = `CA: ${ini.classeArmadura}`;
  document.getElementById('enc-nd').textContent          = `ND: ${ini.nd}`;
  document.getElementById('enc-ataque').textContent      = `Atq: ${ini.ataque}`;

  document.getElementById('encounter-img').src = ini.imagem || '';

  // Badge de categoria
  const badge = document.getElementById('encounter-badge');
  badge.textContent = ini.categoria;
  badge.className = 'encounter-badge' + (ini.categoria === 'Boss Final' ? ' boss-final' : ini.boss ? ' boss' : '');

  // Background
  const bg = document.getElementById('encounter-bg');
  bg.className = ini.categoria === 'Boss Final' ? 'encounter-bg boss-final-bg' : 'encounter-bg';

  // Fala do inimigo (mostrar sempre)
  const speechEl = document.getElementById('encounter-speech');
  const falaEl   = document.getElementById('encounter-fala');
  if (ini.fala) {
    falaEl.textContent = ini.fala;
    speechEl.classList.remove('hidden');
  } else {
    speechEl.classList.add('hidden');
  }
}

// ─── TELA 5: COMBATE ─────────────────────────────────────────────────────
function renderCombat(s) {
  if (!s.jogador || !s.inimigo) return;
  const j   = s.jogador;
  const ini = s.inimigo;
  const t   = s.turno;
  const fb  = s.feedback;

  // Imagens dos combatentes
  document.getElementById('combat-player-img').src = imgClasse(j.classe);
  document.getElementById('combat-enemy-img').src  = ini.imagem || '';

  // Nomes, classe e HP na arena
  document.getElementById('combat-player-name').textContent  = j.nome;
  document.getElementById('combat-player-class').textContent = j.classe;
  document.getElementById('combat-player-hp').textContent    = `HP ${j.hpAtual}/${j.hpMaximo}`;
  document.getElementById('combat-enemy-name').textContent   = ini.nome;
  document.getElementById('combat-enemy-cat').textContent    = ini.categoria || '';
  document.getElementById('combat-enemy-hp').textContent     = `HP ${ini.hpAtual}/${ini.hpMaximo}`;
  document.getElementById('combat-enemy-ca').textContent     = `CA ${ini.classeArmadura}`;

  // Estágio
  document.getElementById('combat-stage-label').textContent = `Estágio ${s.estagioAtual} · ${ini.nome}`;

  // Barras de HP
  document.getElementById('combat-player-hp-fill').style.width      = pct(j.hpAtual, j.hpMaximo) + '%';
  document.getElementById('combat-player-hp-fill').style.background = hpColor(j.hpAtual, j.hpMaximo);
  document.getElementById('combat-enemy-hp-fill').style.width       = pct(ini.hpAtual, ini.hpMaximo) + '%';
  document.getElementById('combat-enemy-hp-fill').style.background  = hpColor(ini.hpAtual, ini.hpMaximo);

  // Feedback central
  renderFeedback(fb);

  // HP tag na coluna direita
  document.getElementById('cb-hp').textContent = `${j.hpAtual}/${j.hpMaximo} HP`;

  // Slots de magia (Mago/Paladino)
  const slotsTotal = j.slotsNivel1 + j.slotsNivel2 + j.slotsNivel3;
  const cbSlots = document.getElementById('cb-slots');
  if (slotsTotal > 0) {
    cbSlots.textContent = `Slots: ${j.slotsNivel1}/${j.slotsNivel2}/${j.slotsNivel3}`;
    cbSlots.classList.remove('hidden');
  } else {
    cbSlots.classList.add('hidden');
  }

  // Cura pelas Mãos (Paladino)
  const cbCura = document.getElementById('cb-cura');
  if (j.classe === 'Paladino') {
    cbCura.textContent = `Cura: ${j.curaPelasMaos}`;
    cbCura.classList.remove('hidden');
    document.getElementById('hint-cura').textContent = `Reserva: ${j.curaPelasMaos}`;
  } else {
    cbCura.classList.add('hidden');
  }

  // Surto de Ação (Guerreiro)
  const cbSurto = document.getElementById('cb-surto');
  if (j.classe === 'Guerreiro') {
    cbSurto.textContent = `Surto: ${j.surtoDeAcao}`;
    cbSurto.classList.remove('hidden');
    document.getElementById('hint-surto').textContent = `${j.surtoDeAcao} uso(s)`;
  } else {
    cbSurto.classList.add('hidden');
  }

  // Poções
  document.getElementById('cb-pocoes').textContent = `🧪 ${j.pocoes}`;
  document.getElementById('hint-pocao').textContent = `${j.pocoes} poções`;

  // Badges de turno
  renderTurnoBadges(t);

  // Botões por classe e estado do turno
  renderActionButtons(j, t);

  // Hints dinâmicos
  if (j.nivel >= 5) {
    document.getElementById('hint-gelo').textContent = '2d8 frio (Nv.5+)';
    document.getElementById('hint-fogo').textContent = '2d10 fogo (Nv.5+)';
  }

  // Log de combate
  renderCombatLog(s.log || []);
}

function renderFeedback(fb) {
  const el = document.getElementById('feedback');
  if (!fb) return;
  el.textContent  = fb.texto || '';
  el.className    = `feedback ${fb.tipo || 'neutro'}`;
  el.classList.add('pop');
  el.addEventListener('animationend', () => el.classList.remove('pop'), { once: true });
}

function renderTurnoBadges(t) {
  const princ = document.getElementById('cb-turno-principal');
  const bonus = document.getElementById('cb-turno-bonus');

  if (t.acoesExtras > 0) {
    princ.textContent  = `Ação Extra ⚡`;
    princ.className    = 'turno-badge turno-extra';
  } else if (t.acaoPrincipalUsada) {
    princ.textContent  = 'Ação ✗';
    princ.className    = 'turno-badge turno-usado';
  } else {
    princ.textContent  = 'Ação ✓';
    princ.className    = 'turno-badge turno-livre';
  }

  if (t.acaoBonusUsada) {
    bonus.textContent = 'Bônus ✗';
    bonus.className   = 'turno-badge turno-usado';
  } else {
    bonus.textContent = 'Bônus ✓';
    bonus.className   = 'turno-badge turno-livre';
  }
}

function renderActionButtons(j, t) {
  const classe     = j.classe;
  const princUsada = t.acaoPrincipalUsada && t.acoesExtras === 0;
  const bonusUsada = t.acaoBonusUsada;

  // Esconde todos os botões de classe primeiro
  document.querySelectorAll('[data-action]').forEach(btn => {
    const cl = btn.classList;
    // Mostra apenas os botões da classe atual + universais
    const visivel = cl.contains('todos')
                 || cl.contains(classe.toLowerCase());

    // Para o Raio de Fogo, só mostra se nível 6+
    if (btn.dataset.action === 'fogo' && j.nivel < 6) {
      btn.classList.add('hidden');
      return;
    }

    cl.toggle('hidden', !visivel);
    if (!visivel) return;

    // Determina se deve estar desabilitado
    const tipo = btn.dataset.tipo;
    let disabled = false;

    if (tipo === 'principal') {
      disabled = princUsada;
    } else if (tipo === 'bonus') {
      disabled = bonusUsada;
    }

    // Regras específicas
    const a = btn.dataset.action;
    if (a === 'smite')    disabled = disabled || j.slotsNivel1 <= 0;
    if (a === 'misil')    disabled = disabled || j.slotsNivel1 <= 0;
    if (a === 'paralisar')disabled = disabled || (j.slotsNivel1 + j.slotsNivel2) <= 0;
    if (a === 'cura')     disabled = disabled || j.curaPelasMaos <= 0 || j.hpAtual >= j.hpMaximo;
    if (a === 'folego')   disabled = disabled || j.hpAtual >= j.hpMaximo;
    if (a === 'surto')    disabled = disabled || j.surtoDeAcao <= 0;
    if (a === 'pocao')    disabled = disabled || j.pocoes <= 0 || j.hpAtual >= j.hpMaximo;
    if (a === 'encerrar') disabled = !t.acaoPrincipalUsada && t.acoesExtras === 0;

    btn.disabled = disabled;

    // Botão encerrar: mantém pulsação se ação foi usada
    if (a === 'encerrar') {
      btn.classList.toggle('pronto', t.acaoPrincipalUsada || t.acoesExtras > 0);
    }
  });

  // Agenda encerramento automático se ação principal foi gasta
  if (t && t.acaoPrincipalUsada && state?.fase === 'combate') {
    scheduleAutoEnd();
  } else if (t && !t.acaoPrincipalUsada) {
    clearAutoEnd();
  }
}

function renderCombatLog(log) {
  const el = document.getElementById('combat-log');
  const novas = log.slice(logCache.length);
  novas.forEach(msg => {
    const p = document.createElement('p');
    p.textContent = msg;
    const ml = msg.toLowerCase();
    if (/turno do inimigo|turno de|iniciativa/.test(ml)) p.classList.add('log-novo-turno');
    else if (/dano|crítico|sofreu|errou|golpe/.test(ml))  p.classList.add('log-dano');
    else if (/recupera|cura|fôlego|\+\d+ hp/.test(ml))   p.classList.add('log-cura');
    else if (/magia|arcana|gelo|fogo|mísseis|paralisado|arcano/.test(ml)) p.classList.add('log-magia');
    else if (/surto|vantagem|buff|resistência|certeiro/.test(ml)) p.classList.add('log-status');
    else if (/inimigo ataca|inimigo usa|ogro|arbusto|lobo|orc/.test(ml)) p.classList.add('log-inimigo');
    else if (/estágio|xp|nível|vitória|derrota/.test(ml)) p.classList.add('log-sistema');
    el.appendChild(p);
    logCache.push(msg);
  });
  el.scrollTop = el.scrollHeight;
}

// ─── TELA 6: EVENTO ──────────────────────────────────────────────────────
function renderEvent(s) {
  const emoji = { descanso: '🏕', loja: '🛒', cutscene: '📖', evento: '📜' };
  document.getElementById('event-icon').textContent  = emoji[s.fase] || '📜';
  document.getElementById('event-title').textContent = s.fase === 'descanso' ? 'Área de Descanso'
                                                      : s.fase === 'loja'    ? 'Mercador'
                                                      : 'Evento';
  document.getElementById('event-desc').textContent  = s.log?.slice(-1)[0] || '';
  document.getElementById('btn-descansar').classList.toggle('hidden', s.fase !== 'descanso');
  document.getElementById('btn-continuar').classList.toggle('hidden', s.fase === 'descanso');
}

// ─── TELA 7: VITÓRIA DE ESTÁGIO ──────────────────────────────────────────
function renderVictory(s) {
  if (!s.jogador) return;
  const j = s.jogador;

  document.getElementById('victory-title').textContent    = `Estágio ${s.estagioAtual} Concluído!`;
  document.getElementById('victory-hp').textContent       = `HP: ${j.hpAtual}/${j.hpMaximo}`;
  document.getElementById('victory-nivel').textContent    = `Nível ${j.nivel}`;
  document.getElementById('victory-xp-total').textContent = `XP: ${j.xp}/${j.xpProximoNivel}`;

  // XP ganho (detectado pelo log)
  const logXp = (s.log || []).find(l => l.includes('XP'));
  const xpGanho = logXp ? logXp.match(/\+(\d+) XP/) : null;
  document.getElementById('victory-xp-value').textContent = xpGanho ? `+${xpGanho[1]}` : '+XP';

  // Level-up
  const levelupEl = document.getElementById('victory-levelup');
  if (j.nivel > nivelAnterior) {
    document.getElementById('victory-level-text').textContent = `Nível ${j.nivel}!`;
    levelupEl.classList.remove('hidden');
    nivelAnterior = j.nivel;
  } else {
    levelupEl.classList.add('hidden');
  }

  // Descanso
  const restText = document.getElementById('victory-rest-text');
  const logDescanso = (s.log || []).find(l => l.toLowerCase().includes('descanso'));
  restText.textContent = logDescanso || '';

  // Log do combate na tela de vitória
  const vlog = document.getElementById('victory-log');
  if (vlog) {
    vlog.innerHTML = '';
    (s.log || []).forEach(msg => {
      const p = document.createElement('p');
      p.textContent = msg;
      const ml = msg.toLowerCase();
      if (/dano|crítico|sofreu|golpe/.test(ml))   p.classList.add('log-dano');
      else if (/cura|fôlego|recupera/.test(ml))   p.classList.add('log-cura');
      else if (/magia|arcana|gelo|fogo/.test(ml)) p.classList.add('log-magia');
      else if (/xp|nível|vitória/.test(ml))        p.classList.add('log-sistema');
      vlog.appendChild(p);
    });
    vlog.scrollTop = vlog.scrollHeight;
  }
}

// ─── TELA 8: FINAL ───────────────────────────────────────────────────────
function renderFinal(s) {
  const vitoria = s.vitoria;
  document.getElementById('final-emblem').textContent = vitoria ? '★' : '💀';
  document.getElementById('final-emblem').className   = vitoria ? 'final-emblem' : 'final-emblem derrota';
  document.getElementById('final-title').textContent  = vitoria ? 'Vitória!' : 'Derrota';

  const msg = vitoria
    ? 'A floresta é purificada e a Pluma de Prata ganha uma nova lenda.'
    : (s.jogador ? `${s.jogador.nome} caiu antes de conter o miasma do Bosque dos Lamentos.` : 'Derrota.');
  document.getElementById('final-result').textContent = msg;

  if (s.jogador) {
    document.getElementById('final-nivel').textContent  = s.jogador.nivel;
    document.getElementById('final-kills').textContent  = s.inimigosDerrotados;
    document.getElementById('final-estagio').textContent= `${s.estagioAtual} / ${s.estagioMax}`;
  }

  const ul = document.getElementById('final-paths');
  ul.innerHTML = '';
  (s.caminhosPercorridos || []).forEach(c => {
    const li = document.createElement('li');
    li.textContent = c;
    ul.appendChild(li);
  });
}

// ─── FICHA LATERAL ────────────────────────────────────────────────────────
function renderSheet(j) {
  if (!j) return;
  const el = document.getElementById('sheet-content');
  const rows = [
    ['Classe',    j.classe],
    ['Nível',     j.nivel],
    ['HP',        `${j.hpAtual}/${j.hpMaximo}`],
    ['CA',        j.classeArmadura],
    ['XP',        `${j.xp}/${j.xpProximoNivel}`],
    ['Ouro',      `${j.ouro} 🪙`],
    ['Arma',      j.arma],
    ['Armadura',  j.armadura],
    ['Poções',    j.pocoes],
    ['FOR',       j.atributos?.forca],
    ['DES',       j.atributos?.destreza],
    ['CON',       j.atributos?.constituicao],
    ['INT',       j.atributos?.inteligencia],
    ['SAB',       j.atributos?.sabedoria],
    ['CAR',       j.atributos?.carisma],
  ];
  if (j.curaPelasMaos)  rows.push(['Cura c/ Mãos', j.curaPelasMaos]);
  if (j.surtoDeAcao)    rows.push(['Surto de Ação', j.surtoDeAcao]);
  if (j.slotsNivel1 + j.slotsNivel2 + j.slotsNivel3 > 0)
    rows.push(['Slots Magia', `${j.slotsNivel1}/${j.slotsNivel2}/${j.slotsNivel3}`]);

  el.innerHTML = rows.map(([k, v]) => `
    <div class="sheet-row">
      <span>${k}</span>
      <strong>${v ?? '—'}</strong>
    </div>
  `).join('');
}

// ─── TOASTS ───────────────────────────────────────────────────────────────
function toast(msg) {
  const cont = document.getElementById('toast-container');
  const el = document.createElement('div');
  el.className = 'toast';
  el.textContent = msg;
  cont.appendChild(el);
  setTimeout(() => el.remove(), 3200);
}

// ─── HELPERS ──────────────────────────────────────────────────────────────
function pct(v, max) { return max > 0 ? Math.max(0, Math.min(100, (v / max) * 100)) : 0; }

function hpColor(v, max) {
  const p = pct(v, max);
  if (p > 60) return 'linear-gradient(90deg,#10b981,#34d399)';
  if (p > 30) return 'linear-gradient(90deg,#f59e0b,#fbbf24)';
  return 'linear-gradient(90deg,#ef4444,#f87171)';
}

function imgClasse(classe) {
  return { Guerreiro: 'assets/guerreiro.png', Mago: 'assets/mago.png', Paladino: 'assets/paladino.png' }[classe]
      || 'assets/paladino.png';
}

// ─── ANIMAÇÕES DE COMBATE ─────────────────────────────────────────────────
function shakeCombatant(side) {
  const el = document.getElementById(side === 'player' ? 'combat-player-img' : 'combat-enemy-img');
  el.classList.remove('shake');
  void el.offsetWidth; // reflow
  el.classList.add('shake');
}

// ─── EVENTOS DE BOTÕES ────────────────────────────────────────────────────

// TELA INTRO
document.getElementById('btn-nova-aventura').addEventListener('click', async () => {
  render(await post(API.novo));
});

// TELA MAPA
document.getElementById('btn-avancar').addEventListener('click', async () => {
  render(await post(API.avancar));
});

// TELA ENCONTRO
document.getElementById('btn-iniciar-combate').addEventListener('click', async () => {
  logCache.length = 0; // limpa log ao iniciar combate
  document.getElementById('combat-log').innerHTML = '';
  render(await post(API.combate));
});

// TELA COMBATE — Ações
document.querySelectorAll('[data-action]').forEach(btn => {
  btn.addEventListener('click', async () => {
    const acao = btn.dataset.action;
    const prev = state;

    // Se é encerrar, cancela o timer automático
    if (acao === 'encerrar') clearAutoEnd();

    const s = await post(API.acao, { acao });

    // Animação shake se levou dano ou atacou
    if (s.jogador && prev?.jogador && s.jogador.hpAtual < prev.jogador.hpAtual) shakeCombatant('player');
    if (s.inimigo && prev?.inimigo && s.inimigo.hpAtual < prev.inimigo.hpAtual)  shakeCombatant('enemy');

    render(s);
  });
});

// TELA COMBATE — Ficha
document.getElementById('btn-ficha-combat').addEventListener('click', () => {
  const sheet = document.getElementById('side-sheet');
  sheet.classList.toggle('hidden');
  if (!sheet.classList.contains('hidden') && state?.jogador) {
    renderSheet(state.jogador);
  }
});
document.getElementById('btn-close-sheet').addEventListener('click', () => {
  document.getElementById('side-sheet').classList.add('hidden');
});

// TELA VITÓRIA
document.getElementById('btn-proximo-estagio').addEventListener('click', async () => {
  render(await post(API.evento, { acao: 'continuar' }));
});

// TELA EVENTO
document.getElementById('btn-descansar').addEventListener('click', async () => {
  render(await post(API.evento, { acao: 'descansar' }));
});
document.getElementById('btn-continuar').addEventListener('click', async () => {
  render(await post(API.evento, { acao: 'continuar' }));
});

// TELA FINAL
document.getElementById('btn-reiniciar').addEventListener('click', async () => {
  nivelAnterior = 1;
  logCache.length = 0;
  document.getElementById('combat-log').innerHTML = '';
  render(await post(API.novo));
});

// ─── INICIALIZAÇÃO ────────────────────────────────────────────────────────
(async () => {
  // Tenta carregar estado do servidor (jogo em andamento)
  try {
    const s = await fetch(API.estado).then(r => r.json());
    if (s.fase && s.fase !== 'criacao') {
      nivelAnterior = s.jogador?.nivel || 1;
      render(s);
    } else {
      // Mostra a tela de intro animada
      showScreen('intro', true);
    }
  } catch {
    showScreen('intro', true);
  }
})();
