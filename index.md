---
layout: page
title: Welcome!
---
{% include JB/setup %}

This is my blog. There are many more like it, but this one is mine. If you want to know more about me, check out the [about](about.html) page. Otherwise, [kick back, relax, take a load off your mind](http://www.youtube.com/watch?v=BxHNztg0X3s).
<hr />

<div class="posts">
  {% for post in site.posts %}
  <h3 style="line-height:32px">
    <small>{{ post.date | date_to_string }}</small><br />
    {{ post.title }}
  </h3>
    <div style="margin-left:1em;">{{ post.excerpt }}
    <a href="{{ BASE_PATH }}{{ post.url }}">Read more</a></div>
  {% endfor %}
</div>


