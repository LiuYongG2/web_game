const express = require('express');
const path = require('path');

const app = express();
const PORT = process.env.PORT || 3000;

app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, 'views'));

app.use(express.static(path.join(__dirname, 'public')));
app.use('/assets', express.static(path.join(__dirname, 'assets')));
app.use('/shared', express.static(path.join(__dirname, 'shared')));
app.use('/games', express.static(path.join(__dirname, 'games')));
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

app.get('/', (req, res) => {
  res.render('index', { title: '游戏平台' });
});

app.get('/about', (req, res) => {
  res.render('about', { title: '关于' });
});

app.get('/api/hello', (req, res) => {
  res.json({ status: 'ok', message: 'Hello from MyWeb API!', timestamp: new Date().toISOString() });
});

app.use((req, res) => {
  res.status(404).render('404', { title: '页面未找到' });
});

app.listen(PORT, () => {
  console.log(`Server running at http://localhost:${PORT}`);
});
