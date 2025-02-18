import axios from 'axios';

const apiClient = axios.create({
	baseURL: 'http://192.168.1.163:5187/',
	headers: {
		'Content-Type': 'application/json',
	},
	withCredentials: true,
});

export default apiClient;
