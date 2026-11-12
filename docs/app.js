(() => {
  'use strict';

  const copy = {
    th:{back:'← กลับไปพอร์ต',demo:'เดโมระบบจองจัดเลี้ยง',title:'จัดเมนูสำหรับงานของคุณ',description:'เลือกอาหาร วันจัดงาน และจำนวนแขก ระบบจะคำนวณราคาก่อนสร้างรายการจองตัวอย่าง',notice:'เดโมรองรับ 2–50 คน รายการจองอยู่ในหน้านี้และจะหายเมื่อโหลดหน้าใหม่ ไม่มีการจองหรือชำระเงินจริง',menu:'เมนูอาหาร',menuHint:'ราคาต่อคน เลือกได้หลายรายการ',date:'วันจัดงาน',guests:'จำนวนแขก',quote:'สรุปราคา',perPerson:'อาหารต่อคน',food:'ค่าอาหาร',fee:'ค่าดำเนินการ',total:'ยอดรวม',submit:'สร้างรายการจองตัวอย่าง',reservation:'รายการจองตัวอย่าง',pending:'รอการยืนยัน',cancelled:'ยกเลิกแล้ว',cancel:'ยกเลิกรายการจองตัวอย่าง',chooseMenu:'เลือกอาหารอย่างน้อยหนึ่งรายการ'},
    en:{back:'← Back to portfolio',demo:'Catering booking demo',title:'Plan your event menu.',description:'Pick your dishes, event date, and guest count to see a quote and create a sample reservation.',notice:'The demo covers 2–50 guests. Reservations reset when you reload. No bookings or payments are sent.',menu:'Menu',menuHint:'Prices are per guest. Select one or more dishes.',date:'Event date',guests:'Guests',quote:'Price estimate',perPerson:'Food per guest',food:'Food subtotal',fee:'Service fee',total:'Estimated total',submit:'Create sample reservation',reservation:'Sample reservation',pending:'Pending confirmation',cancelled:'Cancelled',cancel:'Cancel sample reservation',chooseMenu:'Choose at least one dish.'}
  };
  Object.assign(copy.th,{customer:'ฝั่งลูกค้า',admin:'ฝั่งผู้ดูแล',adminBookings:'จัดการการจอง',adminMenu:'จัดการเมนูอาหาร',filter:'กรองสถานะ',all:'ทั้งหมด',confirmed:'ยืนยันแล้ว',confirm:'ยืนยันรายการ',empty:'ยังไม่มีรายการ ลองสร้างการจองในฝั่งลูกค้า',editDish:'เลือกเมนู',newDish:'เพิ่มเมนูใหม่',dishName:'ชื่อเมนู',dishCategory:'หมวดหมู่',dishPrice:'ราคาต่อคน',saveDish:'บันทึกเมนู',dishSaved:'บันทึกเมนูแล้ว ลองกลับไปดูที่ฝั่งลูกค้า'});
  Object.assign(copy.en,{customer:'Customer',admin:'Admin',adminBookings:'Manage bookings',adminMenu:'Manage dishes',filter:'Filter by status',all:'All',confirmed:'Confirmed',confirm:'Confirm booking',empty:'No bookings here. Create a booking in the customer view.',editDish:'Select dish',newDish:'Add a dish',dishName:'Dish name',dishCategory:'Category',dishPrice:'Price per guest',saveDish:'Save dish',dishSaved:'Dish saved. View the updated catalog as a customer.'});
  const dishes = [
    {id:'sushi',th:'ซูชิแซลมอน',en:'Salmon sushi',category:'Sushi',price:120},
    {id:'roll',th:'โรลแซลมอน',en:'Salmon roll',category:'Sushi',price:150},
    {id:'soup',th:'ซุปมิโซะ',en:'Miso soup',category:'Soup',price:50},
    {id:'salad',th:'สลัดผัก',en:'Garden salad',category:'Salad',price:80}
  ];
  const feeTiers = [[10,5000],[20,9000],[35,12000],[50,18000]];
  const $ = id => document.getElementById(id);
  const key = 'ohm-portfolio-language';
  let lang = 'th';
  let booking;
  const bookings = [];
  let needsMenu = false;
  try { if (localStorage.getItem(key) === 'en') lang = 'en'; } catch {}
  const tr = name => copy[lang][name];
  const money = amount => new Intl.NumberFormat(lang,{style:'currency',currency:'THB',currencyDisplay:'narrowSymbol',maximumFractionDigits:0}).format(amount);
  const dishName = dish => dish.name ?? dish[lang];

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
    $('booked-menu').textContent = booking.chosen.map(dishName).join(', ');
    $('booked-total').textContent = money(booking.total);
    $('status').textContent = tr(booking.status);
    $('cancel').disabled = booking.status !== 'pending';
  }

  function renderAdmin() {
    const filter = $('admin-filter').value;
    const list = $('admin-bookings');
    list.replaceChildren();
    const visible = bookings.filter(row => filter === 'all' || row.status === filter);
    if (!visible.length) {
      const message = document.createElement('p');
      message.textContent = tr('empty');
      list.append(message);
    }
    for (const row of visible) {
      const item = document.createElement('article');
      item.className = 'admin-row';
      const details = document.createElement('div');
      const headline = document.createElement('strong');
      headline.textContent = `EP-${row.id} · ${row.date} · ${money(row.total)}`;
      const description = document.createElement('p');
      description.textContent = `${row.chosen.map(dishName).join(', ')} · ${tr(row.status)}`;
      details.append(headline,description);
      item.append(details);
      if (row.status === 'pending') {
        const actions = document.createElement('div');
        actions.className = 'admin-actions';
        for (const [status,label] of [['confirmed','confirm'],['cancelled','cancel']]) {
          const button = document.createElement('button');
          button.type = 'button';
          button.dataset.booking = row.id;
          button.dataset.status = status;
          button.textContent = tr(label);
          actions.append(button);
        }
        item.append(actions);
      }
      list.append(item);
    }
  }

  function renderMenu() {
    const checked = new Set([...$('menu').querySelectorAll(':checked')].map(input => input.id));
    const selection = $('edit-dish').value;
    $('menu').innerHTML = dishes.map(dish => `<label><input id="${dish.id}" type="checkbox"><span><strong id="name-${dish.id}"></strong><small id="price-${dish.id}"></small></span></label>`).join('');
    $('edit-dish').replaceChildren(new Option(tr('newDish'),''));
    for (const dish of dishes) {
      $(dish.id).checked = checked.has(dish.id);
      $(`name-${dish.id}`).textContent = dishName(dish);
      $(`price-${dish.id}`).textContent = `${dish.category} · ${money(dish.price)}`;
      $('edit-dish').append(new Option(dishName(dish),dish.id));
    }
    $('edit-dish').value = selection;
  }

  function renderLanguage() {
    document.documentElement.lang = lang;
    document.querySelectorAll('[data-copy]').forEach(el => { el.textContent = tr(el.dataset.copy); });
    $('language').lang = lang === 'th' ? 'en' : 'th';
    $('language').textContent = lang === 'th' ? 'EN' : 'TH';
    renderMenu();
    renderQuote();
    renderBooking();
    renderAdmin();
  }

  document.querySelectorAll('[data-role]').forEach(button => button.addEventListener('click', () => {
    const admin = button.dataset.role === 'admin';
    $('admin-view').hidden = !admin;
    $('customer-view').hidden = admin;
    document.querySelectorAll('[data-role]').forEach(tab => tab.setAttribute('aria-pressed',String(tab === button)));
    if (admin) renderAdmin();
  }));
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
    booking = {...estimate,id:bookings.length+1,chosen:estimate.chosen.map(dish => ({...dish})),date:$('date').value,status:'pending'};
    bookings.unshift(booking);
    $('reservation').hidden = false;
    renderBooking();
    $('reservation-title').focus();
  });
  $('cancel').addEventListener('click', () => {
    booking.status = 'cancelled';
    renderBooking();
    $('reservation-title').focus();
  });
  $('admin-filter').addEventListener('change',renderAdmin);
  $('admin-bookings').addEventListener('click',event => {
    const button = event.target.closest('[data-booking]');
    if (!button) return;
    const row = bookings.find(item => item.id === Number(button.dataset.booking));
    if (row?.status !== 'pending') return;
    row.status = button.dataset.status;
    renderAdmin();
    renderBooking();
    $('admin-filter').focus();
  });
  $('edit-dish').addEventListener('change',() => {
    const dish = dishes.find(item => item.id === $('edit-dish').value);
    $('dish-name').value = dish ? dishName(dish) : '';
    $('dish-category').value = dish?.category ?? '';
    $('dish-price').value = dish?.price ?? '';
    $('menu-status').textContent = '';
  });
  $('menu-form').addEventListener('submit',event => {
    event.preventDefault();
    const name = $('dish-name').value.trim();
    const category = $('dish-category').value.trim();
    if (!name || !category) return;
    const price = $('dish-price').valueAsNumber;
    const dish = dishes.find(item => item.id === $('edit-dish').value);
    if (dish) Object.assign(dish,{name,category,price});
    else dishes.push({id:`dish-${dishes.length}`,name,category,price});
    renderMenu();
    renderQuote();
    $('menu-status').textContent = tr('dishSaved');
  });

  renderLanguage();
})();
