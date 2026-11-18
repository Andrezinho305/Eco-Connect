# Origem dos efeitos sonoros

Todos os sons ficam em `Assets/Resources/SFX/` e são tocados pelo `SFXManager`.
Nenhum exige pagamento ou crédito.

## Gravações reais — Openverse / Freesound (CC0 1.0, domínio público)

| Arquivo | Som original | Autor | Link |
|---|---|---|---|
| build_water.wav | waterbloop.mp3 | kstargio | https://freesound.org/people/kstargio/sounds/611642 |
| build_electric.wav | magnets crackling together (trecho) | gecop | https://freesound.org/people/gecop/sounds/522116 |
| build_sewage.wav | Thud2.wav | BMacZero | https://freesound.org/people/BMacZero/sounds/96137 |
| victory.wav | correct.mp3 | lionelmatthew001 | https://freesound.org/people/lionelmatthew001/sounds/538333 |
| ambient_city_birds.ogg | city morning with birds and traffic (30 s, em loop) | thegreatbelow | https://freesound.org/people/thegreatbelow/sounds/546794 |

## Síntese em código — banco do earshot (código MIT, sons gerados localmente)

| Arquivo | Som do banco |
|---|---|
| erase.wav | swipe/paper |
| no_money.wav | ui_pop/fall |
| service_connected.wav | pro_notify/ding_1822 |
| house_complete.wav | mallet/flourish |
| star_1 / star_2 / star_3.wav | mallet/marimba em C5 / E5 / G5 |
| click.wav | pro_click/soft |
| tool_switch.wav | fit/click/floss_lid |
| panel_open.wav / panel_close.wav | pro_pop/bloop / pro_pop/boop |
| tutorial_step.wav | notify/mid_wood |

## Tratamento

Volumes balanceados por categoria (LUFS momentâneo máximo, teto de −1 dBFS):
construção ≈ −18 a −21, apagar −21, sem dinheiro −19, serviço −19, casa completa −15,
vitória −13, estrelas −16, clique −25, ferramenta −25, painel −23, tutorial −20.
Ambiente: −32 LUFS integrado (~15 dB abaixo da música), loop com crossfade de 2 s.
