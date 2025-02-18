import axios from 'axios';

const axiosInstance = axios.create({
  baseURL: 'http://192.168.1.163:5187/api',  
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

export default axiosInstance;
