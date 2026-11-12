(() => {
  'use strict';

  const copy = {
    th:{back:'← กลับไปพอร์ต',demo:'เดโมระบบจองจัดเลี้ยง',title:'จัดเมนูสำหรับงานของคุณ',description:'เลือกอาหาร วันจัดงาน และจำนวนแขก ระบบจะคำนวณราคาก่อนสร้างรายการจองตัวอย่าง',notice:'เดโมรองรับ 2–50 คน รายการจองอยู่ในหน้านี้และจะหายเมื่อโหลดหน้าใหม่ ไม่มีการจองหรือชำระเงินจริง',menu:'เมนูอาหาร',menuHint:'ราคาต่อคน เลือกได้หลายรายการ',date:'วันจัดงาน',guests:'จำนวนแขก',quote:'สรุปราคา',perPerson:'อาหารต่อคน',food:'ค่าอาหาร',fee:'ค่าดำเนินการ',total:'ยอดรวม',submit:'สร้างรายการจองตัวอย่าง',reservation:'รายการจองตัวอย่าง',pending:'รอการยืนยัน',cancelled:'ยกเลิกแล้ว',cancel:'ยกเลิกรายการจองตัวอย่าง',chooseMenu:'เลือกอาหารอย่างน้อยหนึ่งรายการ'},
    en:{back:'← Back to portfolio',demo:'Catering booking demo',title:'Plan your event menu.',description:'Pick your dishes, event date, and guest count to see a quote and create a sample reservation.',notice:'The demo covers 2–50 guests. Reservations reset when you reload. No bookings or payments are sent.',menu:'Menu',menuHint:'Prices are per guest. Select one or more dishes.',date:'Event date',guests:'Guests',quote:'Price estimate',perPerson:'Food per guest',food:'Food subtotal',fee:'Service fee',total:'Estimated total',submit:'Create sample reservation',reservation:'Sample reservation',pending:'Pending confirmation',cancelled:'Cancelled',cancel:'Cancel sample reservation',chooseMenu:'Choose at least one dish.'}
  };
  const dishes = [
    {id:'sushi',th:'ซูชิแซลมอน',en:'Salmon sushi',price:120},
    {id:'roll',th:'โรลแซลมอน',en:'Salmon roll',price:150},
    {id:'soup',th:'ซุปมิโซะ',en:'Miso soup',price:50},
    {id:'salad',th:'สลัดผัก',en:'Garden salad',price:80}
  ];
  const feeTiers = [[10,5000],[20,9000],[35,12000],[50,18000]];
  const $ = id => document.getElementById(id);
  const key = 'ohm-portfolio-language';
  let lang = 'th';
  let booking;
  let needsMenu = false;
  try { if (localStorage.getItem(key) === 'en') lang = 'en'; } catch {}
  const tr = name => copy[lang][name];
  const money = amount => new Intl.NumberFormat(lang,{style:'currency',currency:'THB',currencyDisplay:'narrowSymbol',maximumFractionDigits:0}).format(amount);

  function quote() {
    const chosen = dishes.filter(dish => $(dish.id).checked);
    const guests = $('guests').valueAsNumber;
    const perGuest = chosen.reduce((sum,dish) => sum + dish.price,0);
    const fee = $('guests').validity.valid ? feeTiers.find(([max]) => guests <= max)[1] : null;
    return {chosen,guests,perGuest,fee,total: fee === null ? null : perGuest * guests + fee};
  }

  function renderQuote() {
    const {chosen,guests,perGuest,fee,total} = quote();
    $('per-person').textContent = money(perGuest);
    $('food').textContent = fee === null ? '—' : money(perGuest * guests);
    $('fee').textContent = fee === null ? '—' : money(fee);
    $('total').textContent = total === null || !chosen.length ? '—' : money(total);
    $('form-error').textContent = needsMenu && !chosen.length ? tr('chooseMenu') : '';
  }

  function renderBooking() {
    if (!booking) return;
    const [year,month,day] = booking.date.split('-').map(Number);
    $('booked-date').textContent = new Intl.DateTimeFormat(lang,{dateStyle:'long',calendar:'gregory'}).format(new Date(year,month-1,day));
    $('booked-guests').textContent = booking.guests;
    $('booked-menu').textContent = booking.chosen.map(dish => dish[lang]).join(', ');
    $('booked-total').textContent = money(booking.total);
    $('status').textContent = tr(booking.status);
    $('cancel').disabled = booking.status === 'cancelled';
  }

  function renderLanguage() {
    document.documentElement.lang = lang;
    document.querySelectorAll('[data-copy]').forEach(el => { el.textContent = tr(el.dataset.copy); });
    $('language').lang = lang === 'th' ? 'en' : 'th';
    $('language').textContent = lang === 'th' ? 'EN' : 'TH';
    for (const dish of dishes) {
      $(`name-${dish.id}`).textContent = dish[lang];
      $(`price-${dish.id}`).textContent = money(dish.price);
    }
    renderQuote();
    renderBooking();
  }

  $('menu').innerHTML = dishes.map(dish => `<label><input id="${dish.id}" type="checkbox"><span><strong id="name-${dish.id}"></strong><small id="price-${dish.id}"></small></span></label>`).join('');
  const today = new Date();
  $('date').min = [today.getFullYear(),today.getMonth()+1,today.getDate()].map(n => String(n).padStart(2,'0')).join('-');
  $('language').addEventListener('click', () => {
    lang = lang === 'th' ? 'en' : 'th';
    try { localStorage.setItem(key,lang); } catch {}
    renderLanguage();
  });
  $('booking-form').addEventListener('input',renderQuote);
  $('booking-form').addEventListener('submit', event => {
    event.preventDefault();
    const estimate = quote();
    needsMenu = !estimate.chosen.length;
    renderQuote();
    if (needsMenu) {
      $('menu').querySelector('input').focus();
      return;
    }
    booking = {...estimate,date:$('date').value,status:'pending'};
    $('reservation').hidden = false;
    renderBooking();
    $('reservation-title').focus();
  });
  $('cancel').addEventListener('click', () => {
    booking.status = 'cancelled';
    renderBooking();
    $('reservation-title').focus();
  });

  renderLanguage();
})();
